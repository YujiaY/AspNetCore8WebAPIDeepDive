﻿using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController(
    ICourseLibraryRepository courseLibraryRepository,
    IMapper mapper) : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
    private readonly IMapper _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));

    [HttpGet]
    [HttpHead]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(
       [FromQuery(Name = "mainCategory")] string? mainCategory = "",
       [FromQuery(Name = "searchQuery")] string? searchQuery = "")
    { 
        // throw new Exception("Test exception");
        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(mainCategory, searchQuery); 

        // return them
        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
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
