using AutoMapper;
using FarmEntity = TpaSodManagement.Database.Entities.Farm;
using CustomerEntity = TpaSodManagement.Database.Entities.Customer;
using SaleEntity = TpaSodManagement.Database.Entities.Sale;
using SeedingEntity = TpaSodManagement.Database.Entities.Seeding;
using ProductEntity = TpaSodManagement.Database.Entities.Product;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.ViewModels;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<FarmEntity, FarmViewModel>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.OrganizationName))
            .ForMember(dest => dest.AreaTypeName, opt => opt.MapFrom(src => src.AreaType.AreaTypeName));

        CreateMap<CustomerEntity, CustomerViewModel>()
            .ForMember(dest => dest.PersonName, opt => opt.MapFrom(src => 
                src.Person != null ? src.Person.FirstName + " " + src.Person.LastName : null))
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.OrganizationName));

        CreateMap<SaleEntity, SaleViewModel>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                 src.Customer.Person != null
                     ? (src.Customer.Person.FirstName + " " + src.Customer.Person.LastName).Trim()
                     : (src.Customer.Organization != null ? src.Customer.Organization.OrganizationName : "")))
            .ForMember(dest => dest.SaleTypeName, opt => opt.MapFrom(src => src.SaleType.SaleTypeName))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency.CurrencyCode))
            .ForMember(dest => dest.LineItems, opt => opt.MapFrom(src => src.SaleLineItems));

        CreateMap<SaleLineItem, SaleLineItemViewModel>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName));

        CreateMap<SeedingEntity, SeedingViewModel>()
            .ForMember(dest => dest.FarmName, opt => opt.MapFrom(src => src.Farm.Organization.OrganizationName))
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field.FieldName))
            .ForMember(dest => dest.TagRangeCode, opt => opt.MapFrom(src => src.TagRange.TagRangeCode))
            .ForMember(dest => dest.AreaTypeName, opt => opt.MapFrom(src => src.AreaType.AreaTypeName));

        CreateMap<ProductEntity, ProductViewModel>()
            .ForMember(dest => dest.ProductCategoryName, opt => opt.MapFrom(src => src.ProductCategory.CategoryName))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency.CurrencyCode));
    }
}