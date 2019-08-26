using System.Collections.Generic;
using System.Threading.Tasks;
using TestBed.Services.Models;

namespace TestBed.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<GraphUser>> GetUsers(string groupId, string token, int syncDurationInHours = 0);
    }
}
