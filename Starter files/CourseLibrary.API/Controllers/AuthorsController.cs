using System.Text.Json;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController(
    IPropertyMappingService propertyMappingService,
    ICourseLibraryRepository courseLibraryRepository,
    IMapper mapper) : ControllerBase
{
    private readonly IPropertyMappingService _propertyMappingService = propertyMappingService ??
            throw new ArgumentNullException(nameof(propertyMappingService));
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

            _ => Url.Link(nameof(GetAuthors),
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
        
        // get authors from repo
        PageList<Author> authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorsResourceParameters);

        string? previousPageLink = authorsFromRepo.HasPreviousPage
            ? CreateAuthorResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage)
            : null;

        string? nextPageLink = authorsFromRepo.HasNextPage
            ? CreateAuthorResourceUri(authorsResourceParameters, ResourceUriType.NextPage)
            : null;

        var paginationMetadata = new
        {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            currentPage = authorsFromRepo.CurrentPage,
            totalPages = authorsFromRepo.TotalPages,
            previousPageLink,
            nextPageLink,
        };
        
        Response.Headers.Add("X-Pagination", 
            JsonSerializer.Serialize(paginationMetadata));

        // return them
        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo)
                .ShapeData(authorsResourceParameters.Fields));
    }

    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<ActionResult<AuthorDto>> GetAuthor(Guid authorId)
    {
        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        // return author
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto authorDto)
    {
        var authorEntity = _mapper.Map<Entities.Author>(authorDto);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        return CreatedAtRoute("GetAuthor",
            new { authorId = authorToReturn.Id },
            authorToReturn);
    }

    [HttpOptions]
    public IActionResult GetAuthorsOptions()
    {
        Response.Headers.Add("Allow", "GET,HEAD,POST,OPTIONS");
        Response.Headers.Add("AllowMethod", "GET,HEAD,POST,OPTIONS");
        return Ok();
    }
}
