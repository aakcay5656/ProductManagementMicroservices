using AutoMapper;
using Product.Application.DTOs;
using Product.Application.Features.Commands;
using Product.Application.Features.Queries;

namespace Product.Application.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<ProductQueryDto, GetProductsQuery>();

            // Entity to DTO mappings
            CreateMap<Product.Core.Entities.Product, ProductResponseDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Command to Entity mappings
            CreateMap<CreateProductCommand, Product.Core.Entities.Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Product.Core.Enums.ProductStatus.Active))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => GenerateSlug(src.Name)));

            CreateMap<UpdateProductCommand, Product.Core.Entities.Product>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // UserId de�i�tirilemez
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => GenerateSlug(src.Name)));

            // DTO mappings
            CreateMap<CreateProductDto, CreateProductCommand>();
            CreateMap<UpdateProductDto, UpdateProductCommand>();
        }

        private static string GenerateSlug(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            return name.ToLowerInvariant()
                      .Replace(" ", "-")
                      .Replace("�", "c")
                      .Replace("�", "g")
                      .Replace("�", "i")
                      .Replace("�", "o")
                      .Replace("�", "s")
                      .Replace("�", "u")
                      .Replace("�", "i")
                      .Replace("�", "c")
                      .Replace("�", "g")
                      .Replace("�", "o")
                      .Replace("�", "s")
                      .Replace("�", "u")
                      .Trim('-');
        }
    }
}
