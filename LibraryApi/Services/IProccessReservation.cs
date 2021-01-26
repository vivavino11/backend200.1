using LibraryApi.Domain;
using System.Threading.Tasks;

namespace LibraryApi.Services
{
    public interface IProccessReservation
    {
        Task ProcessReservation(Reservation reservation);
    }
}