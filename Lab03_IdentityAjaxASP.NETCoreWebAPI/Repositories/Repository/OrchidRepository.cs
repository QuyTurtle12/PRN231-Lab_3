using BusinessObjects;
using DataAccess.DTO.Orchid;
using DataAccess.Interface;
using DataAccess.Paginated;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Repository
{
    public class OrchidRepository : IOrchidRepository
    {
        private readonly IUOW _unitOfWork;

        public OrchidRepository(IUOW unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task DeleteAsync(int id)
        {
            // Get the existing orchid by ID
            Orchid? existingOrchid = _unitOfWork.GetDAO<Orchid>()
                .Entities
                .FirstOrDefault(o => o.OrchidId == id);

            // Validate if the orchid exists
            if (existingOrchid == null) throw new KeyNotFoundException($"Orchid with ID {id} not found.");

            // Remove the existing orchid from the database
            await _unitOfWork.GetDAO<Orchid>().DeleteAsync(existingOrchid);
            await _unitOfWork.SaveAsync();
        }

        public async Task<PaginatedList<Orchid>> GetAllAsync(int pageIndex, int pageNumber, string? idSearch, string? nameSearch, string? categorySearch, bool? isNatural)
        {
            // Validate inputs
            if (pageIndex < 1) pageIndex = 1;
            if (pageNumber < 1) pageNumber = 10;

            // Initialize the queryable collection from the DAO
            IQueryable<Orchid> query = _unitOfWork.GetDAO<Orchid>().Entities;

            // Apply Filters
            if (!string.IsNullOrWhiteSpace(idSearch))
            {
                query = query.Where(o => o.OrchidId.ToString() == idSearch);
            }

            if (!string.IsNullOrEmpty(nameSearch))
            {
                query = query.Where(o => o.OrchidName.Contains(nameSearch));
            }

            if (!string.IsNullOrEmpty(categorySearch))
            {
                query = query.Where(o => o.Category != null && o.Category.CategoryName.Contains(categorySearch));
            }

            if (isNatural.HasValue)
            {
                query = query.Where(o => o.IsNatural == isNatural.Value);
            }

            query.OrderByDescending(o => o.OrchidName);

            // Convert query to paginated list
            PaginatedList<Orchid> result = await _unitOfWork.GetDAO<Orchid>().GetPagging(query, pageIndex, pageNumber);

            return result;
        }

        public async Task InsertAsync(CreateOrchidDTO orchid)
        {
            Orchid newOrchid = new Orchid
            {
                OrchidName = orchid.OrchidName,
                CategoryId = orchid.CategoryId,
                IsNatural = orchid.IsNatural,
                OrchidDescription = orchid.OrchidDescription,
                OrchidUrl = orchid.OrchidUrl,
                Price = orchid.Price
            };

            // Get last OrchidId from the database to assign a new ID
            int lastIdOrchid = _unitOfWork.GetDAO<Orchid>()
                .Entities
                .Max(o => o.OrchidId);

            newOrchid.OrchidId = lastIdOrchid + 1;

            // Insert the new orchid into the database
            await _unitOfWork.GetDAO<Orchid>().InsertAsync(newOrchid);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(int id, UpdateOrchidDTO orchid)
        {
            // Get the existing orchid by ID
            Orchid? existingOrchid = _unitOfWork.GetDAO<Orchid>()
                .Entities
                .FirstOrDefault(o => o.OrchidId == id);

            // Validate if the orchid exists
            if (existingOrchid == null) throw new KeyNotFoundException($"Orchid with ID {id} not found.");

            // Update the properties of the existing orchid
            existingOrchid.OrchidDescription = orchid.OrchidDescription;
            existingOrchid.OrchidName = orchid.OrchidName;
            existingOrchid.OrchidUrl = orchid.OrchidUrl;
            existingOrchid.Price = orchid.Price;
            existingOrchid.IsNatural = orchid.IsNatural;
            existingOrchid.CategoryId = orchid.CategoryId;

            // Update the existing orchid to the database
            await _unitOfWork.GetDAO<Orchid>().UpdateAsync(existingOrchid);
            await _unitOfWork.SaveAsync();
        }
    }
}
