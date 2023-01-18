using KeeMind.Services.Data;

namespace KeeMind.Services
{
    public interface IRepository
    {
        bool ArchiveExists();

        Task<DatabaseContext?> TryOpenArchive(string PIN);

        Task CreateArchive(string PIN);

        DatabaseContext OpenArchive();

        void DeleteArchive();

        void CloseArchive();
    }
  
}
