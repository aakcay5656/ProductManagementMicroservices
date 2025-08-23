using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.DTOs;
using Product.Application.Features.Commands;
using Product.Application.Features.Queries;
using System.Security.Claims;

namespace Product.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IMediator mediator, IMapper mapper, ILogger<ProductsController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all active products with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryDto queryDto)
        {
            try
            {
                var query = _mapper.Map<GetProductsQuery>(queryDto);
                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.ErrorMessage, errors = result.Errors });
                }

                return Ok(new
                {
                    message = "Products retrieved successfully",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        
        /// Get product by ID
     
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var query = new GetProductByIdQuery { Id = id };
                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    return NotFound(new { message = result.ErrorMessage });
                }

                return Ok(new
                {
                    message = "Product retrieved successfully",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID: {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// Get current user's products
        [HttpGet("my-products")]
        [Authorize]
        public async Task<IActionResult> GetMyProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var query = new GetUserProductsQuery
                {
                    UserId = userId,
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm
                };

                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.ErrorMessage, errors = result.Errors });
                }

                return Ok(new
                {
                    message = "User products retrieved successfully",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user products");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// Create a new product
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                // Manual validation check
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        );

                    _logger.LogWarning("Validation failed: {@ValidationErrors}", errors);

                    return BadRequest(new
                    {
                        message = "Validation failed",
                        errors = errors
                    });
                }

                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var command = _mapper.Map<CreateProductCommand>(createProductDto);
                command.UserId = userId;

                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    _logger.LogError("Product creation failed: {ErrorMessage}", result.ErrorMessage);

                    return BadRequest(new
                    {
                        message = result.ErrorMessage,
                        errors = result.Errors
                    });
                }

                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = result.Data!.Id },
                    new
                    {
                        message = "Product created successfully",
                        data = result.Data
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }


        /// Update an existing product
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var command = _mapper.Map<UpdateProductCommand>(updateProductDto);
                command.Id = id;
                command.UserId = userId;

                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    if (result.ErrorMessage.Contains("not found"))
                        return NotFound(new { message = result.ErrorMessage });

                    if (result.ErrorMessage.Contains("not authorized"))
                        return Forbid();

                    return BadRequest(new
                    {
                        message = result.ErrorMessage,
                        errors = result.Errors
                    });
                }

                return Ok(new
                {
                    message = "Product updated successfully",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// Delete a product
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var command = new DeleteProductCommand { Id = id, UserId = userId };
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    if (result.ErrorMessage.Contains("not found"))
                        return NotFound(new { message = result.ErrorMessage });

                    if (result.ErrorMessage.Contains("not authorized"))
                        return Forbid();

                    return BadRequest(new { message = result.ErrorMessage });
                }

                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// Search products by name or description
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetProductsQuery
                {
                    SearchTerm = q,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.ErrorMessage });
                }

                return Ok(new
                {
                    message = "Search completed successfully",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with term: {SearchTerm}", q);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// Get products by category
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetProductsByCategory(Core.Enums.ProductCategory category, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetProductsQuery
                {
                    Category = category,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.ErrorMessage });
                }

                return Ok(new
                {
                    message = "Products retrieved successfully",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by category: {Category}", category);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("sub")?.Value;

            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return 0;
        }
    }
}
