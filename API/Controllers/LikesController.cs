using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interface;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(IUnitOfWork unitToWork) : BaseApiController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        var sourceUserId = User.GetUserId();

        if(sourceUserId == targetUserId) return BadRequest("You cannot like yourself");
        var existingLike = await unitToWork.LikesRepository.GetUserLike(sourceUserId,targetUserId);

        if(existingLike == null){
            var like = new UserLike{
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId
            };

            unitToWork.LikesRepository.AddLike(like);
        }else{
            unitToWork.LikesRepository.DeleteLike(existingLike);
        }

        if (await unitToWork.Complete()) return Ok();

        return BadRequest("Failed to update like");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds() {
        return Ok(await unitToWork.LikesRepository.GetCurrentUserLikeIds(User.GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery]LikesParams likesParams){
        likesParams.UserId =User.GetUserId();
        var users = await unitToWork.LikesRepository.GetUserLikes(likesParams);

        Response.AddPaginationHeader(users);
        return Ok(users);
    }
}