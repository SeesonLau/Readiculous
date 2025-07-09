using Readiculous.Data.Interfaces;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using AutoMapper;
using System.Collections.Generic;

namespace Readiculous.Services.Services
{
    public class GuestViewService : IGuestViewService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public GuestViewService(IBookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public GuestViewModel LoadGuestViewModel()
        {
            var newBooks = _bookRepository.GetLatestBooks();
            var topBooks = _bookRepository.GetTopRatedBooks();

            var newBookVMs = _mapper.Map<List<BookViewModel>>(newBooks);
            var topBookVMs = _mapper.Map<List<BookViewModel>>(topBooks);

            return new GuestViewModel
            {
                NewBooks = newBookVMs,
                TopBooks = topBookVMs
            };
        }
    }
}
