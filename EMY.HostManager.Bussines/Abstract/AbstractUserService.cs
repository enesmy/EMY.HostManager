using EMY.HostManager.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMY.HostManager.Bussines.Abstract
{
    public abstract class AbstractUserService
    {
        public abstract Task<User> GetByUserID(int userID);
        public abstract Task<IEnumerable<User>> GetAll();
        public abstract Task Add(User newUser, int adderRef);
        public abstract Task Update(User user, int updaterRef);
        public abstract Task DeActivate(int userID, int deactivaterRef);
        public abstract Task Activate(int userID, int activaterRef);
        public abstract Task<IEnumerable<string>> GetAllRoles(int userID);
        public abstract Task AddRole(UserRole newRole, int adderRef);
        public abstract Task RemoveRole(int userID, string formName, AuthType type, int removerRef);
        public abstract Task ClearAllRoles(int userID, int removerRef);
        public abstract Task<ResultModel> CheckLoginUser(string userName, string password);
        public abstract Task ChangePassword(int userID, string newPassword);
        public abstract Task<bool> CheckRoleIsExist(int userID, string formName, AuthType type);
        public abstract Task<User> GetUserByUserName(string userName);
    }
}
