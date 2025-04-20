using API.DTO;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(d=>d.Age, o=>o.MapFrom(s=>s.DateOfbirth.CalculateAge()))
            .ForMember(d=>d.PhotoUrl, o=>
                o.MapFrom(s=>s.Photos.FirstOrDefault(x=>x.IsMain)!.Url));
        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto,AppUser>();
        CreateMap<RegisterDto,AppUser>();
        CreateMap<string,DateOnly>().ConstructUsing(s=>DateOnly.Parse(s));
        CreateMap<Message,MessageDto>()
            .ForMember(d => d.SenderPhotoUrl, o 
                => o.MapFrom(s=> s.Sender.Photos.FirstOrDefault(x=>x.IsMain)!.Url))
            .ForMember(d => d.RecipientPhotoUrl, o 
                => o.MapFrom(s=> s.Recipient.Photos.FirstOrDefault(x=>x.IsMain)!.Url));
        CreateMap<DateTime, DateTime>().ConvertUsing(d=>DateTime.SpecifyKind(d, DateTimeKind.Utc));
        CreateMap<DateTime?, DateTime?>().ConvertUsing(d=> d.HasValue 
            ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
    }
}

