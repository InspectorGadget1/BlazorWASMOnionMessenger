﻿using AutoMapper;
using BlazorWASMOnionMessenger.Domain.DTOs.Participant;
using BlazorWASMOnionMessenger.Domain.Entities;

namespace BlazorWASMOnionMessenger.Application.Common.MappingProfiles
{
    public class ParticipantMappingProfile : Profile
    {
        public ParticipantMappingProfile()
        {
            CreateMap<Participant, ParticipantDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.ApplicationUser.UserName));
            CreateMap<CreateParticipantDto, Participant>();
        }
    }
}
