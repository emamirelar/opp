using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.Specifications
{
    public class BusinessTestPartnerSpecification : BaseSpecification<Partner>
    {
        public BusinessTestPartnerSpecification(bool matchAll = true) : base(matchAll ? (p => true) : (p => false))
        {
        }
        
        public BusinessTestPartnerSpecification(string status) : base(p => p.Status.ToString() == status)
        {
        }
        
        public BusinessTestPartnerSpecification(Expression<Func<Partner, bool>> criteria) : base(criteria)
        {
        }
    }
    
    public class TestUNOPSPartnerSpecification : BaseSpecification<UNOPSPartner>
    {
        public TestUNOPSPartnerSpecification(bool matchAll = true) : base(matchAll ? (p => true) : (p => false))
        {
        }
        
        public TestUNOPSPartnerSpecification(string status) : base(p => p.Status.ToString() == status)
        {
        }
        
        public TestUNOPSPartnerSpecification(Expression<Func<UNOPSPartner, bool>> criteria) : base(criteria)
        {
        }
    }
}