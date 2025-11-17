using AutoMapper;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.ViewModels;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Farm, FarmViewModel>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.OrganizationName))
            .ForMember(dest => dest.AreaTypeName, opt => opt.MapFrom(src => src.AreaType.AreaTypeName));

        CreateMap<Customer, CustomerViewModel>()
            .ForMember(dest => dest.PersonName, opt => opt.MapFrom(src => 
                src.Person != null ? src.Person.FirstName + " " + src.Person.LastName : null))
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.OrganizationName));

        CreateMap<Sale, SaleViewModel>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CustomerType == "PERSON" 
                 ? src.Customer.Person.FirstName + " " + src.Customer.Person.LastName 
                 : src.Customer.Organization.OrganizationName))
            .ForMember(dest => dest.SaleTypeName, opt => opt.MapFrom(src => src.SaleType.SaleTypeName))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency.CurrencyCode))
            .ForMember(dest => dest.LineItems, opt => opt.MapFrom(src => src.SaleLineItems));

        CreateMap<SaleLineItem, SaleLineItemViewModel>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName));

        CreateMap<Seeding, SeedingViewModel>()
            .ForMember(dest => dest.FarmName, opt => opt.MapFrom(src => src.Farm.Organization.OrganizationName))
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field.FieldName))
            .ForMember(dest => dest.TagRangeCode, opt => opt.MapFrom(src => src.TagRange.TagRangeCode))
            .ForMember(dest => dest.AreaTypeName, opt => opt.MapFrom(src => src.AreaType.AreaTypeName));

        CreateMap<Product, ProductViewModel>()
            .ForMember(dest => dest.ProductCategoryName, opt => opt.MapFrom(src => src.ProductCategory.CategoryName))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency.CurrencyCode));
    }
}