using AutoMapper;
using AutoMapper.QueryableExtensions;
using LibraryApi.Domain;
using LibraryApi.Filters;
using LibraryApi.Models.Reservations;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryApi.Controllers
{
    public class ReservationsController : ControllerBase
    {
        private readonly LibraryDataContext _context;
        private readonly IMapper _mapper;
        private MapperConfiguration _config;
        private readonly IProccessReservation _reservationProcessor;

        public ReservationsController(IMapper mapper, LibraryDataContext context, MapperConfiguration config, IProccessReservation reservationProcessor)
        {
            _mapper = mapper;
            _context = context;
            _config = config;
            _reservationProcessor = reservationProcessor;
        }


        [HttpPost("/reservations")]
        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 5)]
        [ValidateModel]
        public async Task<ActionResult> CreateReservationAsync([FromBody] PostReservationRequest request)
        {
            var reservation = _mapper.Map<Reservation>(request);


            reservation.Status = ReservationStatus.Pending;
            await _reservationProcessor.ProcessReservation(reservation);
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<GetReservationDetailsResponse>(reservation);

            return CreatedAtRoute("reservations#getreservation", new { id = response.Id }, response);
        }

        // GET /reservations/{id}
        [HttpGet("/reservations/{id:int}", Name = "reservations#getreservation")]
        public async Task<ActionResult<GetReservationDetailsResponse>> GetAReservation(int id)
        {
            var response = await _context.Reservations
                .ProjectTo<GetReservationDetailsResponse>(_config)
                .SingleOrDefaultAsync(r => r.Id == id);

            return this.Maybe(response);
        }
        [HttpPost("/reservations/accepted")]
        public async Task<ActionResult> ReservationAccepted([FromBody] GetReservationDetailsResponse request)
        {
            var reservation = await _context.Reservations
                .SingleOrDefaultAsync(r => r.Id == request.Id);
            if (reservation == null)
            {
                return BadRequest("No pending Reservation with that Id");
            }
            else
            {
                reservation.Status = ReservationStatus.Accepted;
                await _context.SaveChangesAsync();
            }
            return Accepted();
        }

        [HttpPost("/reservation/rejected")]
        public async Task<ActionResult> ReservationRejected([FromBody] GetReservationDetailsResponse request)
        {
            var reservation = await _context.Reservations
    .SingleOrDefaultAsync(r => r.Id == request.Id);
            if (reservation == null)
            {
                return BadRequest("No pending Reservation with that Id");
            }
            else
            {
                reservation.Status = ReservationStatus.Rejected;
                await _context.SaveChangesAsync();
            }
            return Accepted();
        }


        [HttpGet("/reservations/pending")]
        public async Task<ActionResult> GetPendingReservations()
        {
            var reservations = await _context.Reservations
                .Where(res => res.Status == ReservationStatus.Pending)
                .ToListAsync();
            return Ok(new { data = reservations });
           
        }

        [HttpGet("/reservations/accepted")]
        public async Task<ActionResult> GetAcceptedReservations()
        {
            var reservations = await _context.Reservations
                .Where(res => res.Status == ReservationStatus.Accepted)
                .ToListAsync();
            return Ok(new { data = reservations });

        }

        [HttpGet("/reservations/rejected")]
        public async Task<ActionResult> GetRejectedReservations()
        {
            var reservations = await _context.Reservations
                .Where(res => res.Status == ReservationStatus.Rejected)
                .ToListAsync();
            return Ok(new { data = reservations });

        }

        [HttpGet("/reservations")]
        public async Task<ActionResult> GetAllReservations()
        {
            throw new NotImplementedException();
        }
    }

}
