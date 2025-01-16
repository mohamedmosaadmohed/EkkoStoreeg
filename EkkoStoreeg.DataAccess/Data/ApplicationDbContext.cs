using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using EkkoSoreeg.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace EkkoSoreeg.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options){ }
        public virtual DbSet<Catagory> TbCatagory { get; set; }
        public virtual DbSet<Product> TbProduct { get; set; }
        public virtual DbSet<ApplicationUser> TbApplicationUser { get; set; }
        public virtual DbSet<ShoppingCart> TbShoppingCarts { get; set; }
        public virtual DbSet<OrderHeader> TbOrderHeaders { get; set; }
        public virtual DbSet<OrderDetails> TbOrderDetails { get; set; }
        public virtual DbSet<ProductColor> TbProductColors { get; set; }
        public virtual DbSet<ProductSize> TbProductSizes { get; set; }
        public virtual DbSet<ProductColorMapping> TbProductColorMappings { get; set; }
        public virtual DbSet<ProductSizeMapping> TbProductSizeMappings { get; set; }
        public virtual DbSet<ProductImage> TbProductImages { get; set; }
        public virtual DbSet<Review> Tbreviews { get; set; }
        public virtual DbSet<Region> TbRegion { get; set; }
        public virtual DbSet<City> TbCity { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("TbUserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("TbUserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("TbUserLogins");
            modelBuilder.Entity<IdentityRole>().ToTable("TbRoles");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("TbRoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("TbUserTokens");
            modelBuilder.Entity<ApplicationUser>().ToTable("TbUsers");
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProductColorMapping>()
            .HasOne(pcm => pcm.Product)
            .WithMany(p => p.ProductColorMappings)
            .HasForeignKey(pcm => pcm.ProductId);

            modelBuilder.Entity<ProductColorMapping>()
                .HasOne(pcm => pcm.ProductColor)
                .WithMany(pc => pc.ProductColorMappings)
                .HasForeignKey(pcm => pcm.ProductColorId);


            modelBuilder.Entity<ProductSizeMapping>()
                .HasOne(pcm => pcm.Product)
                .WithMany(p => p.ProductSizeMappings)
                .HasForeignKey(pcm => pcm.ProductId);

            modelBuilder.Entity<ProductSizeMapping>()
                .HasOne(pcm => pcm.ProductSize)
                .WithMany(pc => pc.ProductSizeMappings)
                .HasForeignKey(pcm => pcm.ProductSizeId);
        }
    }
}
