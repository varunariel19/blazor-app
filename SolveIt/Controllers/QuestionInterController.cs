using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolveIt.Services;
using System.Security.Claims;

namespace SolveIt.Controllers
{
    [ApiController]
    [Route("api")]
    public class QuestionInteractionController : ControllerBase
    {
        private readonly QuestionInteractionService _service;

        public QuestionInteractionController(QuestionInteractionService service)
        {
            _service = service;
        }

        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

     
        [Authorize]
        [HttpPost("questions/{questionId:guid}/{userId:guid}/vote")]
        public async Task<IActionResult> VoteOnQuestion(Guid questionId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("You must be logged in to vote.");



            try
            {
                var result = await _service.VoteOnQuestionAsync(questionId  ,Guid.Parse(userId));

                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

     
        [Authorize]
        [HttpPost("questions/{questionId:guid}/comments")]
        public async Task<IActionResult> AddComment(Guid questionId, [FromBody] AddCommentRequest request)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("You must be logged in to comment.");

            if (string.IsNullOrWhiteSpace(request?.Body))
                return BadRequest("Comment body is required.");

            try
            {
                var result = await _service.AddCommentAsync(questionId, userId, request.Body);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

       
        [Authorize]
        [HttpPost("solutions/{solId:guid}/like")]
        public async Task<IActionResult> LikeSolution(Guid solId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("You must be logged in to like.");

            try
            {
                var result = await _service.LikeSolutionAsync(solId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

       
        [Authorize]
        [HttpPost("solutions/{solId:guid}/dislike")]
        public async Task<IActionResult> DislikeSolution(Guid solId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("You must be logged in to dislike.");

            try
            {
                var result = await _service.DislikeSolutionAsync(solId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }


        [Authorize]
        [HttpPost("questions/{questionId:guid}/solutions")]
        public async Task<IActionResult> AddSolution(Guid questionId, [FromBody] AddSolutionRequest request)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("You must be logged in to post an answer.");

            if (request == null || string.IsNullOrWhiteSpace(request.Body))
                return BadRequest("Solution body is required.");

            try
            {
                var result = await _service.AddSolutionAsync(questionId, userId, request.Body);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }


    }

    public class AddCommentRequest
    {
        public string Body { get; set; } = string.Empty;
    }

    public class AddSolutionRequest
    {
        public string Body { get; set; } = string.Empty;
    }
}
