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
            CreateMap<PolicyType, resPolicyTypeDTO>().ForAllOtherMembers(opt => opt.Ignore());
            CreateMap<reqPolicyTypeDTO, PolicyType>().ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<Policy, resPolicyDTO>().ForAllOtherMembers(opt => opt.Ignore());
            CreateMap<reqPolicyDTO, Policy>().ForAllOtherMembers(opt => opt.Ignore());
        }
       
    }
}
