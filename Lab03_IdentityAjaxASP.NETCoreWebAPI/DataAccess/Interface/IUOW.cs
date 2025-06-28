namespace DataAccess.Interface
{
    public interface IUOW : IDisposable
    {
        IGenericDAO<T> GetDAO<T>() where T : class;
        void Save();
        Task SaveAsync();
        void BeginTransaction();
        void CommitTransaction();
        void RollBack();
    }
}