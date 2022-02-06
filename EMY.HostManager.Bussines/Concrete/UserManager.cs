using EMY.HostManager.Bussines.Abstract;
using EMY.HostManager.DataAccess.Abstract;
using EMY.HostManager.DataAccess.Concrete;
using EMY.HostManager.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMY.HostManager.Bussines.Concrete
{
    public class UserManager : AbstractUserService
    {
        private IAsyncRepository<User> repository = null;
        private IAsyncRepository<UserRole> userRoleRepository = null;
        public UserManager(DbContext context)
        {
            this.repository = new GenericRepository<User>(context);
            this.userRoleRepository = new GenericRepository<UserRole>(context);
        }
        public override async Task Add(User newUser, int adderRef)
        {
            await repository.Add(newUser, adderRef);
        }

        public override async Task DeActivate(int userID, int deactivaterRef)
        {
            var res = await GetByUserID(userID);
            res.IsActive = false;
            await repository.Update(res, deactivaterRef);
        }

        public override async Task Activate(int userID, int activaterRef)
        {
            var res = await GetByUserID(userID);
            res.IsActive = true;
            await repository.Update(res, activaterRef);
        }

        public override async Task<IEnumerable<User>> GetAll()
        {
            var result = await repository.GetAll();
            return result;
        }

        public override async Task<User> GetByUserID(int userID)
        {
            var result = await repository.GetByPrimaryKey(userID);
            return result;
        }

        public override async Task Update(User user, int updaterRef)
        {
            await repository.Update(user, updaterRef);
        }

        public override async Task<IEnumerable<string>> GetAllRoles(int userID)
        {
            var user = await repository.GetByPrimaryKey(userID);
            if (user == null || !user.IsActive || user.IsDeleted) return (new string[] { });
            var AllRoles = await userRoleRepository.GetWhere(o => !o.IsDeleted && o.UserID == userID);
            List<string> roles = new List<string>();
            return AllRoles.ToList().Select(o => o.GetAuthCode);
        }

        public override async Task AddRole(UserRole newRole, int adderRef)
        {
            await userRoleRepository.Add(newRole, adderRef);
        }


        public override async Task ClearAllRoles(int userID, int removerRef)
        {
            var roles = await userRoleRepository.GetWhere(o => !o.IsDeleted && o.UserID == userID);
            foreach (var role in roles)
            {
                await userRoleRepository.Remove(role.UserRoleID, removerRef);
            }
        }

        public override async Task<ResultModel> CheckLoginUser(string userName, string password)
        {
            ResultModel resultModel = new ResultModel();

            var user = await repository.FirstOrDefault(o => !o.IsDeleted && o.UserName.ToLower() == userName.ToLower());

            resultModel.IsSuccess = false;
            if (user == null)
            {
                resultModel.Message = "This user is not exist!";
                resultModel.resultType = -1;
            }
            else if (!user.PasswordControl(password))
            {
                resultModel.Message = "This user password is wrong!";
                resultModel.resultType = 0;
            }
            else
            {
                resultModel.Data = user;
                resultModel.IsSuccess = user.IsActive;
                resultModel.Message = user.IsActive ? "User can log in!" : "User is deactivated!";
                resultModel.resultType = user.IsActive ? 1 : 0;

            }

            return resultModel;
        }

        public override async Task RemoveRole(int userID, string formName, AuthType type, int removerRef)
        {
            var AllRoles = await userRoleRepository.GetWhere(o => !o.IsDeleted && o.UserID == userID && o.AuthorizeType == type && o.FormName == formName);
            if (AllRoles.Count() > 0)
                await userRoleRepository.Remove(AllRoles.First().UserRoleID, removerRef);
        }

        public override async Task ChangePassword(int userID, string newPassword)
        {
            var currentUser = await repository.FirstOrDefault(o => o.UserID == userID);
            if (currentUser != null)
            {
                currentUser.Password = newPassword;
                await repository.Update(currentUser, currentUser.UserID);
            }
        }

        public override async Task<bool> CheckRoleIsExist(int userID, string formName, AuthType type)
        {
            var AllRoles = await userRoleRepository.GetWhere(o => !o.IsDeleted && o.UserID == userID && o.AuthorizeType == type && o.FormName == formName);
            return (AllRoles.Count() > 0);

        }

        public override async Task<User> GetUserByUserName(string userName)
        {
            var user = await repository.FirstOrDefault(o => o.UserName.ToLower() == userName.ToLower() && !o.IsDeleted);
            return user;
        }
    }
}
