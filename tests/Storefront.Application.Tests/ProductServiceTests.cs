using Moq;
using Storefront.Domain;
using Xunit;

namespace Storefront.Application.Tests;

public sealed class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repository = new();

    private ProductService CreateSut() => new(_repository.Object);

    [Fact]
    public async Task CreateAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        Task Act() => sut.CreateAsync(null!);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(Act);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        Task Act() => sut.CreateAsync(new CreateProductRequest { Name = "   ", Price = 1m });

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        Assert.Equal("request", ex.ParamName);
    }

    [Fact]
    public async Task CreateAsync_NegativePrice_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        Task Act() => sut.CreateAsync(new CreateProductRequest { Name = "A", Price = -0.01m });

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        Assert.Equal("request", ex.ParamName);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_AddsProductViaRepository()
    {
        // Arrange
        var sut = CreateSut();
        Product? captured = null;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (Product p, CancellationToken _) =>
                {
                    captured = p;
                    return p;
                });

        // Act
        var dto = await sut.CreateAsync(
            new CreateProductRequest
            {
                Name = "  Keyboard  ",
                Price = 129.99m,
                Description = "  Nice  ",
                IsActive = true
            });

        // Assert
        Assert.NotNull(captured);
        Assert.Equal("Keyboard", captured!.Name);
        Assert.Equal(129.99m, captured.Price);
        Assert.Equal("Nice", captured.Description);
        Assert.True(captured.IsActive);
        Assert.NotEqual(Guid.Empty, captured.Id);

        Assert.Equal(captured.Id, dto.Id);
        Assert.Equal("Keyboard", dto.Name);
        Assert.Equal(129.99m, dto.Price);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        // Arrange
        var sut = CreateSut();
        var id = Guid.NewGuid();
        _repository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var dto = await sut.GetByIdAsync(id);

        // Assert
        Assert.Null(dto);
    }

    [Fact]
    public async Task GetAllAsync_MapsProductsToDtos()
    {
        // Arrange
        var sut = CreateSut();
        var id = Guid.NewGuid();
        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<Product>
                {
                    new()
                    {
                        Id = id,
                        Name = "Mouse",
                        Price = 10m,
                        Description = null,
                        IsActive = true
                    }
                });

        // Act
        var list = await sut.GetAllAsync();

        // Assert
        Assert.Single(list);
        Assert.Equal(id, list[0].Id);
        Assert.Equal("Mouse", list[0].Name);
    }
}
