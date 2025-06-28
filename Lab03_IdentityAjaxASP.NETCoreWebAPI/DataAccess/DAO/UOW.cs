using DataAccess.Interface;

namespace DataAccess.DAO
{


    public class UOW : IUOW
    {
        private bool disposed = false;
        private readonly ProductManagementDbContext _dbContext;
        public UOW(ProductManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IGenericDAO<T> GetDAO<T>() where T : class
        {
            return new GenericDAO<T>(_dbContext);
        }
        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
        public void Save()
        {
            _dbContext.SaveChanges();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
            }
            disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void BeginTransaction()
        {
            _dbContext.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _dbContext.Database.CommitTransaction();
        }

        public void RollBack()
        {
            _dbContext.Database.RollbackTransaction();
        }

    }
}
