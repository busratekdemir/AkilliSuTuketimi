using akıllısuyönetimi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace akıllısuyönetimi.Services
{
    public interface IMeterService
    {
        // Sadece temel sayaç listesini tanımlıyoruz
        Task<List<Meter>> GetMetersAsync();
    }
}