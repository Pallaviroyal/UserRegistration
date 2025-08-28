namespace backend.Repositories
{
 
        public interface GroupcsRepositoryy : IRepository<Groups>
        {
            Task<Group> GetByNameAsync(string groupName);
            Task<IEnumerable<Group>> GetGroupsByUserAsync(int userId);
        }
    }

