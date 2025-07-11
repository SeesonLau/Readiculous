using AutoMapper;
using Readiculous.Services.ServiceModels;

namespace Readiculous.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<BookListItemViewModel, BookViewModel>();
            CreateMap<BookViewModel, BookListItemViewModel>();

            // Add more mappings as needed
        }
    }
}
