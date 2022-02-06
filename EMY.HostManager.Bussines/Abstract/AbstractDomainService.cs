using EMY.HostManager.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMY.HostManager.Bussines.Abstract
{
    public abstract class AbstractDomainService
    {
        public abstract Task<IEnumerable<domainInformation>> GetDomainList();
        public abstract Task<domainInformation> GetDomainInformationByDomainInformationID(int domainInformationID);
        public abstract Task Add(domainInformation domainInformation, int adderRef);
        public abstract Task Update(domainInformation domainInformation, int updaterRef);
        public abstract Task Delete(domainInformation domainInformation, int deleterRef);
        public abstract Task<domainInformation> GetDomainByName(string domainName);
    }
}
