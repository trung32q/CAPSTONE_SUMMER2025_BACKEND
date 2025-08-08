using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using API.DTO.PolicyDTO;

namespace API.Mapping
{
    public class MappingPolicy: Profile
    {
        public MappingPolicy() {
            CreateMap<PolicyType, resPolicyTypeDTO>();
            CreateMap<reqPolicyTypeDTO, PolicyType>();

            CreateMap<Policy, resPolicyDTO>();
            CreateMap<reqPolicyDTO, Policy>().ForAllOtherMembers(opt => opt.Ignore());
        }
       
    }
}
