using System.Dynamic;
using System.Text.Json;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController(
    ProblemDetailsFactory problemDetailsFactory,
    IPropertyMappingService propertyMappingService,
    IPropertyCheckerService propertyCheckerService,
    ICourseLibraryRepository courseLibraryRepository,
    IMapper mapper) : ControllerBase
{
    private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory ??
            throw new ArgumentNullException(nameof(problemDetailsFactory));
    private readonly IPropertyMappingService _propertyMappingService = propertyMappingService ??
            throw new ArgumentNullException(nameof(propertyMappingService));
    private readonly IPropertyCheckerService _propertyCheckerService = propertyCheckerService ??
            throw new ArgumentNullException(nameof(propertyCheckerService));
    private readonly ICourseLibraryRepository _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
    private readonly IMapper _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));

    private string? CreateAuthorResourceUri(AuthorsResourceParameters authorsResourceParameters,
        ResourceUriType uriType)
    {
        return uriType switch
        {
            ResourceUriType.PreviousPage => Url.Link(nameof(GetAuthors),
                new
                {
                    fields = authorsResourceParameters.Fields,
                    orderBy = authorsResourceParameters.OrderBy,
                    pageNumber = authorsResourceParameters.PageNumber - 1,
                    pageSize = authorsResourceParameters.PageSize,
                    mainCategory = authorsResourceParameters.MainCategory,
                    searchQuery = authorsResourceParameters.SearchQuery,
                }),

            ResourceUriType.NextPage => Url.Link(nameof(GetAuthors),
                new
                {
                    fields = authorsResourceParameters.Fields,
                    orderBy = authorsResourceParameters.OrderBy,
                    pageNumber = authorsResourceParameters.PageNumber + 1,
                    pageSize = authorsResourceParameters.PageSize,
                    mainCategory = authorsResourceParameters.MainCategory,
                    searchQuery = authorsResourceParameters.SearchQuery,
                }),

            ResourceUriType.Current or _ => Url.Link(nameof(GetAuthors),
                new
                {
                    fields = authorsResourceParameters.Fields,
                    orderBy = authorsResourceParameters.OrderBy,
                    pageNumber = authorsResourceParameters.PageNumber,
                    pageSize = authorsResourceParameters.PageSize,
                    mainCategory = authorsResourceParameters.MainCategory,
                    searchQuery = authorsResourceParameters.SearchQuery,
                })
        };
    }

    private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId,
        string? fields)
    {
        var authorLinks = new List<LinkDto>();

        if (string.IsNullOrEmpty(fields))
        {
            authorLinks.Add(
                new(Url.Link("GetAuthor", new { authorId }),
                    "self",
                    "GET"));
        }
        else
        {
            authorLinks.Add(
                new(Url.Link("GetAuthor", new { authorId, fields }),
                    "self",
                    "GET"));
        }
        
        authorLinks.Add(
            new(Url.Link("CreateCourseForAuthor", new { authorId }),
                "create_course_for_author",
                "POST"));
        
        authorLinks.Add(
            new(Url.Link("GetCoursesForAuthor", new { authorId }),
                "courses",
                "GET"));
        
        return authorLinks;
    }
    
    private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters authorsResourceParameters,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        var authorLinks = new List<LinkDto>();

        // Self
        authorLinks.Add(
            new LinkDto(CreateAuthorResourceUri(authorsResourceParameters,
                ResourceUriType.Current),
                "self",
                "GET"));

        if (hasNextPage)
        {
            authorLinks.Add(
                new LinkDto(CreateAuthorResourceUri(authorsResourceParameters,
                        ResourceUriType.NextPage),
                    "nextPage",
                    "GET"));
        }
        
        if (hasPreviousPage)
        {
            authorLinks.Add(
                new LinkDto(CreateAuthorResourceUri(authorsResourceParameters,
                        ResourceUriType.PreviousPage),
                    "previousPage",
                    "GET"));
        }
        
        return authorLinks;
    }
    
    [HttpGet(Name = nameof(GetAuthors))]
    [HttpHead]
    public async Task<IActionResult> GetAuthors(
        [FromQuery] AuthorsResourceParameters authorsResourceParameters)
    { 
        // throw new Exception("Test exception");
        if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(
                authorsResourceParameters.OrderBy))
        {
            return BadRequest("OrderBy query is not valid.");
        }

        if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(
                authorsResourceParameters.Fields))
        {
            return BadRequest(
                _problemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: $"Not all requested data shaping fields exist on " +
                            $"the resource: {authorsResourceParameters.Fields}"));
        }
        
        // get authors from repo
        PageList<Author> authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorsResourceParameters);

        // string? previousPageLink = authorsFromRepo.HasPreviousPage
        //     ? CreateAuthorResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage)
        //     : null;
        //
        // string? nextPageLink = authorsFromRepo.HasNextPage
        //     ? CreateAuthorResourceUri(authorsResourceParameters, ResourceUriType.NextPage)
        //     : null;

        var paginationMetadata = new
        {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            currentPage = authorsFromRepo.CurrentPage,
            totalPages = authorsFromRepo.TotalPages,
            // previousPageLink,
            // nextPageLink,
        };
        
        Response.Headers.Add("X-Pagination", 
            JsonSerializer.Serialize(paginationMetadata));
        
        // Create links
        IEnumerable<LinkDto> links = CreateLinksForAuthors(authorsResourceParameters,
            authorsFromRepo.HasNextPage,
            authorsFromRepo.HasPreviousPage);
        
        IEnumerable<ExpandoObject> shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo)
            .ShapeData(authorsResourceParameters.Fields);

        var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
        {
            var authorAsDictionary = author as IDictionary<string, object?>;
            var authorsLinks = CreateLinksForAuthor(
                (Guid)authorAsDictionary["Id"],
                null);
            authorAsDictionary.Add("Links", authorsLinks);

            return authorAsDictionary;
        });

        var linkedCollectionResource = new
        {
            value = shapedAuthorsWithLinks,
            links
        };

        // return them
        return Ok(linkedCollectionResource);
    }

    [Produces("application/json", 
        "application/vnd.magicit.hateoas+json",
        "application/vnd.magicit.author.full+json", 
        "application/vnd.magicit.author.full.hateoas+json",
        "application/vnd.magicit.author.friendly+json", 
        "application/vnd.magicit.author.friendly.hateoas+json")]
    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<IActionResult> GetAuthor(Guid authorId,
        string? fields,
        [FromHeader(Name = "Accept")] string? mediaType)
    {
        var acceptHeader = HttpContext.Request
            .GetTypedHeaders().Accept;

        // TODO: Try TryParseList
        // if (acceptHeader.Count == 0)
        if (!MediaTypeHeaderValue.TryParse(mediaType,
                out MediaTypeHeaderValue? parsedMediaType))
        {
            return BadRequest(
                _problemDetailsFactory.CreateProblemDetails(HttpContext,
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: "Accept header media type value is not a valid media type."));
        }
        
        if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(
                fields))
        {
            return BadRequest(
                _problemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: $"Not all requested data shaping fields exist on " +
                            $"the resource: {fields}"));
        }
        
        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        var subType = parsedMediaType.SubTypeWithoutSuffix;
        var shouldIncludeLinks = subType.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

        IEnumerable<LinkDto> links = new List<LinkDto>();

        if (shouldIncludeLinks)
        {
            links = CreateLinksForAuthor(authorId, fields);
        }
        
        var primaryMediaType = shouldIncludeLinks ?
            parsedMediaType.SubTypeWithoutSuffix.Substring(
                0, parsedMediaType.SubTypeWithoutSuffix.Length - 8)
            : parsedMediaType.SubTypeWithoutSuffix;
        
        // full Author
        if (primaryMediaType == "vnd.magicit.author.full")
        {
            var fullResourceToReturn = _mapper.Map<AuthorFullDto>(authorFromRepo)
                .ShapeData(fields) as IDictionary<string, object?>;

            if (shouldIncludeLinks)
            {
                fullResourceToReturn.Add("links", links);
            }
            
            return Ok(fullResourceToReturn);
        }
        
        // friendly author
        var friendlyResourceToReturn = _mapper.Map<AuthorDto>(authorFromRepo)
            .ShapeData(fields) as IDictionary<string, object?>;

        if (shouldIncludeLinks)
        {
            friendlyResourceToReturn.Add("links", links);
        }
            
        return Ok(friendlyResourceToReturn);
        // if (parsedMediaType.MediaType == "application/vnd.magicit.hateoas+json")
        // // if (acceptHeader.Any(h =>
        // //         h.MediaType == "application/vnd.magicit.hateoas+json"))
        // {
        //     // Create links
        //     links = CreateLinksForAuthor(authorId, fields);
        //     
        //     // Add
        //     var linkedResourceToReturn = _mapper.Map<AuthorDto>(authorFromRepo)
        //         .ShapeData(fields) as IDictionary<string, object?>;
        //
        //     linkedResourceToReturn.Add("links", links);
        //
        //     // return author
        //     return Ok(linkedResourceToReturn);
        // }
        //
        // return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
    }

    [HttpPost(Name = nameof(CreateAuthor))]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto authorDto)
    {
        var authorEntity = _mapper.Map<Entities.Author>(authorDto);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);
        
        // Create links
        var links = CreateLinksForAuthor(authorToReturn.Id, null);
        
        // Add links
        var linkedResourceToReturn = authorToReturn.ShapeData(null)
            as IDictionary<string, object?>;
        
        linkedResourceToReturn.Add("links", links);

        return CreatedAtRoute("GetAuthor",
            // new { authorId = authorToReturn.Id },
            new { authorId = linkedResourceToReturn["Id"] },
            linkedResourceToReturn);
    }

    [HttpOptions]
    public IActionResult GetAuthorsOptions()
    {
        Response.Headers.Add("Allow", "GET,HEAD,POST,OPTIONS");
        Response.Headers.Add("AllowMethod", "GET,HEAD,POST,OPTIONS");
        return Ok();
    }
}
