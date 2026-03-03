using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Specifications
{
    public class TestPartnerSpecification : BaseSpecification<Partner>
    {
        public TestPartnerSpecification(bool matchAll = true) : base(matchAll ? (p => true) : (p => false))
        {
        }
        
        public TestPartnerSpecification(string status) : base(p => p.Status.ToString() == status)
        {
        }
        
        public TestPartnerSpecification(Expression<Func<Partner, bool>> criteria) : base(criteria)
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
