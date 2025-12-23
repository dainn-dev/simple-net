using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DainnProductEAVManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitProductEAV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category_entity",
                columns: table => new
                {
                    entity_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    path = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    url_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    meta_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    meta_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    meta_keywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_entity", x => x.entity_id);
                    table.ForeignKey(
                        name: "FK_category_entity_category_entity_parent_id",
                        column: x => x.parent_id,
                        principalTable: "category_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "eav_attribute_set",
                columns: table => new
                {
                    attribute_set_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entity_type_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    attribute_set_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eav_attribute_set", x => x.attribute_set_id);
                });

            migrationBuilder.CreateTable(
                name: "eav_attribute_group",
                columns: table => new
                {
                    attribute_group_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    attribute_set_id = table.Column<int>(type: "integer", nullable: false),
                    attribute_group_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eav_attribute_group", x => x.attribute_group_id);
                    table.ForeignKey(
                        name: "FK_eav_attribute_group_eav_attribute_set_attribute_set_id",
                        column: x => x.attribute_set_id,
                        principalTable: "eav_attribute_set",
                        principalColumn: "attribute_set_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_entity",
                columns: table => new
                {
                    entity_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    type_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "simple"),
                    attribute_set_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_entity", x => x.entity_id);
                    table.ForeignKey(
                        name: "FK_product_entity_eav_attribute_set_attribute_set_id",
                        column: x => x.attribute_set_id,
                        principalTable: "eav_attribute_set",
                        principalColumn: "attribute_set_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "eav_attribute",
                columns: table => new
                {
                    attribute_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entity_type_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    attribute_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    backend_type = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    frontend_input = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    frontend_label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_unique = table.Column<bool>(type: "boolean", nullable: false),
                    is_searchable = table.Column<bool>(type: "boolean", nullable: false),
                    is_filterable = table.Column<bool>(type: "boolean", nullable: false),
                    is_comparable = table.Column<bool>(type: "boolean", nullable: false),
                    is_visible_on_front = table.Column<bool>(type: "boolean", nullable: false),
                    is_html_allowed_on_front = table.Column<bool>(type: "boolean", nullable: false),
                    is_used_for_promo_rules = table.Column<bool>(type: "boolean", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: true),
                    default_value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    note = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    attribute_group_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eav_attribute", x => x.attribute_id);
                    table.ForeignKey(
                        name: "FK_eav_attribute_eav_attribute_group_attribute_group_id",
                        column: x => x.attribute_group_id,
                        principalTable: "eav_attribute_group",
                        principalColumn: "attribute_group_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "product_category",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_category", x => new { x.product_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_product_category_category_entity_category_id",
                        column: x => x.category_id,
                        principalTable: "category_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_category_product_entity_product_id",
                        column: x => x.product_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_inventory",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    min_qty = table.Column<int>(type: "integer", nullable: false),
                    min_sale_qty = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    max_sale_qty = table.Column<int>(type: "integer", nullable: false, defaultValue: 10000),
                    is_in_stock = table.Column<bool>(type: "boolean", nullable: false),
                    manage_stock = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    backorders = table.Column<bool>(type: "boolean", nullable: false),
                    notify_stock_qty = table.Column<int>(type: "integer", nullable: true),
                    qty_increments = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_inventory", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_product_inventory_product_entity_product_id",
                        column: x => x.product_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_price",
                columns: table => new
                {
                    price_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    customer_group_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    website_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    price = table.Column<decimal>(type: "numeric(20,6)", precision: 20, scale: 6, nullable: false),
                    special_price = table.Column<decimal>(type: "numeric(20,6)", precision: 20, scale: 6, nullable: true),
                    special_from_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    special_to_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    qty = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_price", x => x.price_id);
                    table.ForeignKey(
                        name: "FK_product_price_product_entity_product_id",
                        column: x => x.product_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_relation",
                columns: table => new
                {
                    relation_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: false),
                    child_id = table.Column<int>(type: "integer", nullable: false),
                    relation_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "super")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_relation", x => x.relation_id);
                    table.ForeignKey(
                        name: "FK_product_relation_product_entity_child_id",
                        column: x => x.child_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_relation_product_entity_parent_id",
                        column: x => x.parent_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_website",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    website_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_website", x => new { x.product_id, x.website_id });
                    table.ForeignKey(
                        name: "FK_product_website_product_entity_product_id",
                        column: x => x.product_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_entity_datetime",
                columns: table => new
                {
                    value_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    attribute_id = table.Column<int>(type: "integer", nullable: false),
                    store_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_entity_datetime", x => x.value_id);
                    table.ForeignKey(
                        name: "FK_product_entity_datetime_eav_attribute_attribute_id",
                        column: x => x.attribute_id,
                        principalTable: "eav_attribute",
                        principalColumn: "attribute_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_entity_datetime_product_entity_entity_id",
                        column: x => x.entity_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_entity_decimal",
                columns: table => new
                {
                    value_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<decimal>(type: "numeric(20,6)", precision: 20, scale: 6, nullable: true),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    attribute_id = table.Column<int>(type: "integer", nullable: false),
                    store_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_entity_decimal", x => x.value_id);
                    table.ForeignKey(
                        name: "FK_product_entity_decimal_eav_attribute_attribute_id",
                        column: x => x.attribute_id,
                        principalTable: "eav_attribute",
                        principalColumn: "attribute_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_entity_decimal_product_entity_entity_id",
                        column: x => x.entity_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_entity_int",
                columns: table => new
                {
                    value_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<int>(type: "integer", nullable: true),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    attribute_id = table.Column<int>(type: "integer", nullable: false),
                    store_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_entity_int", x => x.value_id);
                    table.ForeignKey(
                        name: "FK_product_entity_int_eav_attribute_attribute_id",
                        column: x => x.attribute_id,
                        principalTable: "eav_attribute",
                        principalColumn: "attribute_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_entity_int_product_entity_entity_id",
                        column: x => x.entity_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_entity_text",
                columns: table => new
                {
                    value_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<string>(type: "text", nullable: true),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    attribute_id = table.Column<int>(type: "integer", nullable: false),
                    store_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_entity_text", x => x.value_id);
                    table.ForeignKey(
                        name: "FK_product_entity_text_eav_attribute_attribute_id",
                        column: x => x.attribute_id,
                        principalTable: "eav_attribute",
                        principalColumn: "attribute_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_entity_text_product_entity_entity_id",
                        column: x => x.entity_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_entity_varchar",
                columns: table => new
                {
                    value_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    attribute_id = table.Column<int>(type: "integer", nullable: false),
                    store_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_entity_varchar", x => x.value_id);
                    table.ForeignKey(
                        name: "FK_product_entity_varchar_eav_attribute_attribute_id",
                        column: x => x.attribute_id,
                        principalTable: "eav_attribute",
                        principalColumn: "attribute_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_entity_varchar_product_entity_entity_id",
                        column: x => x.entity_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_media_gallery",
                columns: table => new
                {
                    value_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    attribute_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    media_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "image"),
                    store_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    alt_text = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false),
                    disabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_base_image = table.Column<bool>(type: "boolean", nullable: false),
                    is_small_image = table.Column<bool>(type: "boolean", nullable: false),
                    is_thumbnail = table.Column<bool>(type: "boolean", nullable: false),
                    is_swatch_image = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_media_gallery", x => x.value_id);
                    table.ForeignKey(
                        name: "FK_product_media_gallery_eav_attribute_attribute_id",
                        column: x => x.attribute_id,
                        principalTable: "eav_attribute",
                        principalColumn: "attribute_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_media_gallery_product_entity_product_id",
                        column: x => x.product_id,
                        principalTable: "product_entity",
                        principalColumn: "entity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "eav_attribute_set",
                columns: new[] { "attribute_set_id", "attribute_set_name", "entity_type_id", "sort_order" },
                values: new object[] { 1, "Default", 4, 1 });

            migrationBuilder.InsertData(
                table: "eav_attribute_group",
                columns: new[] { "attribute_group_id", "attribute_group_name", "attribute_set_id", "sort_order" },
                values: new object[,]
                {
                    { 1, "General", 1, 1 },
                    { 2, "Prices", 1, 2 },
                    { 3, "Images", 1, 3 },
                    { 4, "Meta Information", 1, 4 },
                    { 5, "Content", 1, 5 }
                });

            migrationBuilder.InsertData(
                table: "eav_attribute",
                columns: new[] { "attribute_id", "attribute_code", "attribute_group_id", "backend_type", "default_value", "entity_type_id", "frontend_input", "frontend_label", "is_comparable", "is_filterable", "is_html_allowed_on_front", "is_required", "is_searchable", "is_unique", "is_used_for_promo_rules", "is_visible_on_front", "note", "position" },
                values: new object[,]
                {
                    { 1, "name", 1, "varchar", null, 4, "text", "Product Name", false, false, false, true, true, false, false, true, null, null },
                    { 2, "description", 5, "text", null, 4, "textarea", "Description", false, false, true, false, true, false, false, true, null, null },
                    { 3, "short_description", 5, "text", null, 4, "textarea", "Short Description", false, false, true, false, true, false, false, true, null, null },
                    { 4, "price", 2, "decimal", null, 4, "price", "Price", true, true, false, true, true, false, false, true, null, null },
                    { 5, "special_price", 2, "decimal", null, 4, "price", "Special Price", false, true, false, false, false, false, false, true, null, null },
                    { 6, "status", 1, "int", "1", 4, "select", "Status", false, false, false, true, false, false, false, true, null, null },
                    { 7, "visibility", 1, "int", "4", 4, "select", "Visibility", false, false, false, true, false, false, false, true, null, null },
                    { 8, "weight", 1, "decimal", null, 4, "text", "Weight", false, false, false, false, false, false, false, true, null, null },
                    { 9, "url_key", 4, "varchar", null, 4, "text", "URL Key", false, false, false, false, false, true, false, true, null, null },
                    { 10, "meta_title", 4, "varchar", null, 4, "text", "Meta Title", false, false, false, false, false, false, false, true, null, null },
                    { 11, "meta_description", 4, "text", null, 4, "textarea", "Meta Description", false, false, false, false, false, false, false, true, null, null },
                    { 12, "meta_keywords", 4, "text", null, 4, "textarea", "Meta Keywords", false, false, false, false, false, false, false, true, null, null },
                    { 13, "image", 3, "varchar", null, 4, "media_image", "Base Image", false, false, false, false, false, false, false, true, null, null },
                    { 14, "small_image", 3, "varchar", null, 4, "media_image", "Small Image", false, false, false, false, false, false, false, true, null, null },
                    { 15, "thumbnail", 3, "varchar", null, 4, "media_image", "Thumbnail", false, false, false, false, false, false, false, true, null, null },
                    { 16, "color", 1, "int", null, 4, "select", "Color", true, true, false, false, false, false, false, true, null, null },
                    { 17, "size", 1, "int", null, 4, "select", "Size", true, true, false, false, false, false, false, true, null, null },
                    { 18, "manufacturer", 1, "varchar", null, 4, "select", "Manufacturer", false, true, false, false, false, false, false, true, null, null },
                    { 19, "special_from_date", 2, "datetime", null, 4, "date", "Special Price From Date", false, false, false, false, false, false, false, true, null, null },
                    { 20, "special_to_date", 2, "datetime", null, 4, "date", "Special Price To Date", false, false, false, false, false, false, false, true, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_entity_parent_id",
                table: "category_entity",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_eav_attribute_attribute_group_id",
                table: "eav_attribute",
                column: "attribute_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_eav_attribute_entity_type_id_attribute_code",
                table: "eav_attribute",
                columns: new[] { "entity_type_id", "attribute_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_eav_attribute_group_attribute_set_id",
                table: "eav_attribute_group",
                column: "attribute_set_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_category_category_id",
                table: "product_category",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_attribute_set_id",
                table: "product_entity",
                column: "attribute_set_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_sku",
                table: "product_entity",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_datetime_attribute_id",
                table: "product_entity_datetime",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_datetime_entity_id_attribute_id_store_id",
                table: "product_entity_datetime",
                columns: new[] { "entity_id", "attribute_id", "store_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_decimal_attribute_id",
                table: "product_entity_decimal",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_decimal_entity_id_attribute_id_store_id",
                table: "product_entity_decimal",
                columns: new[] { "entity_id", "attribute_id", "store_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_int_attribute_id",
                table: "product_entity_int",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_int_entity_id_attribute_id_store_id",
                table: "product_entity_int",
                columns: new[] { "entity_id", "attribute_id", "store_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_text_attribute_id",
                table: "product_entity_text",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_text_entity_id_attribute_id_store_id",
                table: "product_entity_text",
                columns: new[] { "entity_id", "attribute_id", "store_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_varchar_attribute_id",
                table: "product_entity_varchar",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_entity_varchar_entity_id_attribute_id_store_id",
                table: "product_entity_varchar",
                columns: new[] { "entity_id", "attribute_id", "store_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_media_gallery_attribute_id",
                table: "product_media_gallery",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_media_gallery_product_id",
                table: "product_media_gallery",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_price_product_id_customer_group_id_website_id_qty",
                table: "product_price",
                columns: new[] { "product_id", "customer_group_id", "website_id", "qty" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_relation_child_id",
                table: "product_relation",
                column: "child_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_relation_parent_id_child_id_relation_type",
                table: "product_relation",
                columns: new[] { "parent_id", "child_id", "relation_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_category");

            migrationBuilder.DropTable(
                name: "product_entity_datetime");

            migrationBuilder.DropTable(
                name: "product_entity_decimal");

            migrationBuilder.DropTable(
                name: "product_entity_int");

            migrationBuilder.DropTable(
                name: "product_entity_text");

            migrationBuilder.DropTable(
                name: "product_entity_varchar");

            migrationBuilder.DropTable(
                name: "product_inventory");

            migrationBuilder.DropTable(
                name: "product_media_gallery");

            migrationBuilder.DropTable(
                name: "product_price");

            migrationBuilder.DropTable(
                name: "product_relation");

            migrationBuilder.DropTable(
                name: "product_website");

            migrationBuilder.DropTable(
                name: "category_entity");

            migrationBuilder.DropTable(
                name: "eav_attribute");

            migrationBuilder.DropTable(
                name: "product_entity");

            migrationBuilder.DropTable(
                name: "eav_attribute_group");

            migrationBuilder.DropTable(
                name: "eav_attribute_set");
        }
    }
}
