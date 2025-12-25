using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DainnProductEAV.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductCatalogTablesToPascalCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_category_entity_category_entity_parent_id",
                table: "category_entity");

            migrationBuilder.DropForeignKey(
                name: "FK_eav_attribute_eav_attribute_group_attribute_group_id",
                table: "eav_attribute");

            migrationBuilder.DropForeignKey(
                name: "FK_eav_attribute_group_eav_attribute_set_attribute_set_id",
                table: "eav_attribute_group");

            migrationBuilder.DropForeignKey(
                name: "FK_product_category_category_entity_category_id",
                table: "product_category");

            migrationBuilder.DropForeignKey(
                name: "FK_product_category_product_entity_product_id",
                table: "product_category");

            migrationBuilder.DropForeignKey(
                name: "FK_product_entity_eav_attribute_set_attribute_set_id",
                table: "product_entity");

            migrationBuilder.DropForeignKey(
                name: "FK_product_entity_datetime_eav_attribute_attribute_id",
                table: "product_entity_datetime");

            migrationBuilder.DropForeignKey(
                name: "FK_product_entity_datetime_product_entity_entity_id",
                table: "product_entity_datetime");

            migrationBuilder.DropForeignKey(
                name: "FK_product_entity_decimal_eav_attribute_attribute_id",
                table: "product_entity_decimal");

            migrationBuilder.DropForeignKey(
                name: "FK_product_entity_decimal_product_entity_entity_id",
                table: "product_entity_decimal");

            migrationBuilder.DropForeignKey(
                name: "FK_product_entity_int_eav_attribute_attribute_id",
                table: "product_entity_int");

            migrationBuilder.DropForeignKey(
                name: "FK_product_entity_int_product_entity_entity_id",
                table: "product_entity_int");

            migrationBuilder.DropForeignKey(
                name: "FK_product_entity_text_eav_attribute_attribute_id",
                table: "product_entity_text");

            migrationBuilder.DropForeignKey(
                name: "FK_product_entity_text_product_entity_entity_id",
                table: "product_entity_text");

            migrationBuilder.DropForeignKey(
                name: "FK_product_entity_varchar_eav_attribute_attribute_id",
                table: "product_entity_varchar");

            migrationBuilder.DropForeignKey(
                name: "FK_product_entity_varchar_product_entity_entity_id",
                table: "product_entity_varchar");

            migrationBuilder.DropForeignKey(
                name: "FK_product_inventory_product_entity_product_id",
                table: "product_inventory");

            migrationBuilder.DropForeignKey(
                name: "FK_product_media_gallery_eav_attribute_attribute_id",
                table: "product_media_gallery");

            migrationBuilder.DropForeignKey(
                name: "FK_product_media_gallery_product_entity_product_id",
                table: "product_media_gallery");

            migrationBuilder.DropForeignKey(
                name: "FK_product_price_product_entity_product_id",
                table: "product_price");

            migrationBuilder.DropForeignKey(
                name: "FK_product_relation_product_entity_child_id",
                table: "product_relation");

            migrationBuilder.DropForeignKey(
                name: "FK_product_relation_product_entity_parent_id",
                table: "product_relation");

            migrationBuilder.DropForeignKey(
                name: "FK_product_website_product_entity_product_id",
                table: "product_website");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_website",
                table: "product_website");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_relation",
                table: "product_relation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_price",
                table: "product_price");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_media_gallery",
                table: "product_media_gallery");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_inventory",
                table: "product_inventory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_entity_varchar",
                table: "product_entity_varchar");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_entity_text",
                table: "product_entity_text");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_entity_int",
                table: "product_entity_int");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_entity_decimal",
                table: "product_entity_decimal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_entity_datetime",
                table: "product_entity_datetime");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_entity",
                table: "product_entity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_category",
                table: "product_category");

            migrationBuilder.DropPrimaryKey(
                name: "PK_eav_attribute_set",
                table: "eav_attribute_set");

            migrationBuilder.DropPrimaryKey(
                name: "PK_eav_attribute_group",
                table: "eav_attribute_group");

            migrationBuilder.DropPrimaryKey(
                name: "PK_eav_attribute",
                table: "eav_attribute");

            migrationBuilder.DropPrimaryKey(
                name: "PK_category_entity",
                table: "category_entity");

            migrationBuilder.RenameTable(
                name: "product_website",
                newName: "ProductWebsite");

            migrationBuilder.RenameTable(
                name: "product_relation",
                newName: "ProductRelation");

            migrationBuilder.RenameTable(
                name: "product_price",
                newName: "ProductPrice");

            migrationBuilder.RenameTable(
                name: "product_media_gallery",
                newName: "ProductMediaGallery");

            migrationBuilder.RenameTable(
                name: "product_inventory",
                newName: "ProductInventory");

            migrationBuilder.RenameTable(
                name: "product_entity_varchar",
                newName: "ProductEntityVarchar");

            migrationBuilder.RenameTable(
                name: "product_entity_text",
                newName: "ProductEntityText");

            migrationBuilder.RenameTable(
                name: "product_entity_int",
                newName: "ProductEntityInt");

            migrationBuilder.RenameTable(
                name: "product_entity_decimal",
                newName: "ProductEntityDecimal");

            migrationBuilder.RenameTable(
                name: "product_entity_datetime",
                newName: "ProductEntityDatetime");

            migrationBuilder.RenameTable(
                name: "product_entity",
                newName: "ProductEntity");

            migrationBuilder.RenameTable(
                name: "product_category",
                newName: "ProductCategory");

            migrationBuilder.RenameTable(
                name: "eav_attribute_set",
                newName: "EavAttributeSet");

            migrationBuilder.RenameTable(
                name: "eav_attribute_group",
                newName: "EavAttributeGroup");

            migrationBuilder.RenameTable(
                name: "eav_attribute",
                newName: "EavAttribute");

            migrationBuilder.RenameTable(
                name: "category_entity",
                newName: "CategoryEntity");

            migrationBuilder.RenameIndex(
                name: "IX_product_relation_parent_id_child_id_relation_type",
                table: "ProductRelation",
                newName: "IX_ProductRelation_parent_id_child_id_relation_type");

            migrationBuilder.RenameIndex(
                name: "IX_product_relation_child_id",
                table: "ProductRelation",
                newName: "IX_ProductRelation_child_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_price_product_id_customer_group_id_website_id_qty",
                table: "ProductPrice",
                newName: "IX_ProductPrice_product_id_customer_group_id_website_id_qty");

            migrationBuilder.RenameIndex(
                name: "IX_product_media_gallery_product_id",
                table: "ProductMediaGallery",
                newName: "IX_ProductMediaGallery_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_media_gallery_attribute_id",
                table: "ProductMediaGallery",
                newName: "IX_ProductMediaGallery_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_varchar_entity_id_attribute_id_store_id",
                table: "ProductEntityVarchar",
                newName: "IX_ProductEntityVarchar_entity_id_attribute_id_store_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_varchar_attribute_id",
                table: "ProductEntityVarchar",
                newName: "IX_ProductEntityVarchar_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_text_entity_id_attribute_id_store_id",
                table: "ProductEntityText",
                newName: "IX_ProductEntityText_entity_id_attribute_id_store_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_text_attribute_id",
                table: "ProductEntityText",
                newName: "IX_ProductEntityText_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_int_entity_id_attribute_id_store_id",
                table: "ProductEntityInt",
                newName: "IX_ProductEntityInt_entity_id_attribute_id_store_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_int_attribute_id",
                table: "ProductEntityInt",
                newName: "IX_ProductEntityInt_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_decimal_entity_id_attribute_id_store_id",
                table: "ProductEntityDecimal",
                newName: "IX_ProductEntityDecimal_entity_id_attribute_id_store_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_decimal_attribute_id",
                table: "ProductEntityDecimal",
                newName: "IX_ProductEntityDecimal_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_datetime_entity_id_attribute_id_store_id",
                table: "ProductEntityDatetime",
                newName: "IX_ProductEntityDatetime_entity_id_attribute_id_store_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_datetime_attribute_id",
                table: "ProductEntityDatetime",
                newName: "IX_ProductEntityDatetime_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_sku",
                table: "ProductEntity",
                newName: "IX_ProductEntity_sku");

            migrationBuilder.RenameIndex(
                name: "IX_product_entity_attribute_set_id",
                table: "ProductEntity",
                newName: "IX_ProductEntity_attribute_set_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_category_category_id",
                table: "ProductCategory",
                newName: "IX_ProductCategory_category_id");

            migrationBuilder.RenameIndex(
                name: "IX_eav_attribute_group_attribute_set_id",
                table: "EavAttributeGroup",
                newName: "IX_EavAttributeGroup_attribute_set_id");

            migrationBuilder.RenameIndex(
                name: "IX_eav_attribute_entity_type_id_attribute_code",
                table: "EavAttribute",
                newName: "IX_EavAttribute_entity_type_id_attribute_code");

            migrationBuilder.RenameIndex(
                name: "IX_eav_attribute_attribute_group_id",
                table: "EavAttribute",
                newName: "IX_EavAttribute_attribute_group_id");

            migrationBuilder.RenameIndex(
                name: "IX_category_entity_parent_id",
                table: "CategoryEntity",
                newName: "IX_CategoryEntity_parent_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductWebsite",
                table: "ProductWebsite",
                columns: new[] { "product_id", "website_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductRelation",
                table: "ProductRelation",
                column: "relation_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductPrice",
                table: "ProductPrice",
                column: "price_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductMediaGallery",
                table: "ProductMediaGallery",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductInventory",
                table: "ProductInventory",
                column: "product_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductEntityVarchar",
                table: "ProductEntityVarchar",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductEntityText",
                table: "ProductEntityText",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductEntityInt",
                table: "ProductEntityInt",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductEntityDecimal",
                table: "ProductEntityDecimal",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductEntityDatetime",
                table: "ProductEntityDatetime",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductEntity",
                table: "ProductEntity",
                column: "entity_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductCategory",
                table: "ProductCategory",
                columns: new[] { "product_id", "category_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EavAttributeSet",
                table: "EavAttributeSet",
                column: "attribute_set_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EavAttributeGroup",
                table: "EavAttributeGroup",
                column: "attribute_group_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EavAttribute",
                table: "EavAttribute",
                column: "attribute_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryEntity",
                table: "CategoryEntity",
                column: "entity_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryEntity_CategoryEntity_parent_id",
                table: "CategoryEntity",
                column: "parent_id",
                principalTable: "CategoryEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EavAttribute_EavAttributeGroup_attribute_group_id",
                table: "EavAttribute",
                column: "attribute_group_id",
                principalTable: "EavAttributeGroup",
                principalColumn: "attribute_group_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EavAttributeGroup_EavAttributeSet_attribute_set_id",
                table: "EavAttributeGroup",
                column: "attribute_set_id",
                principalTable: "EavAttributeSet",
                principalColumn: "attribute_set_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategory_CategoryEntity_category_id",
                table: "ProductCategory",
                column: "category_id",
                principalTable: "CategoryEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategory_ProductEntity_product_id",
                table: "ProductCategory",
                column: "product_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntity_EavAttributeSet_attribute_set_id",
                table: "ProductEntity",
                column: "attribute_set_id",
                principalTable: "EavAttributeSet",
                principalColumn: "attribute_set_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntityDatetime_EavAttribute_attribute_id",
                table: "ProductEntityDatetime",
                column: "attribute_id",
                principalTable: "EavAttribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntityDatetime_ProductEntity_entity_id",
                table: "ProductEntityDatetime",
                column: "entity_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntityDecimal_EavAttribute_attribute_id",
                table: "ProductEntityDecimal",
                column: "attribute_id",
                principalTable: "EavAttribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntityDecimal_ProductEntity_entity_id",
                table: "ProductEntityDecimal",
                column: "entity_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntityInt_EavAttribute_attribute_id",
                table: "ProductEntityInt",
                column: "attribute_id",
                principalTable: "EavAttribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntityInt_ProductEntity_entity_id",
                table: "ProductEntityInt",
                column: "entity_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntityText_EavAttribute_attribute_id",
                table: "ProductEntityText",
                column: "attribute_id",
                principalTable: "EavAttribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntityText_ProductEntity_entity_id",
                table: "ProductEntityText",
                column: "entity_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntityVarchar_EavAttribute_attribute_id",
                table: "ProductEntityVarchar",
                column: "attribute_id",
                principalTable: "EavAttribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntityVarchar_ProductEntity_entity_id",
                table: "ProductEntityVarchar",
                column: "entity_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInventory_ProductEntity_product_id",
                table: "ProductInventory",
                column: "product_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMediaGallery_EavAttribute_attribute_id",
                table: "ProductMediaGallery",
                column: "attribute_id",
                principalTable: "EavAttribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMediaGallery_ProductEntity_product_id",
                table: "ProductMediaGallery",
                column: "product_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPrice_ProductEntity_product_id",
                table: "ProductPrice",
                column: "product_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductRelation_ProductEntity_child_id",
                table: "ProductRelation",
                column: "child_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductRelation_ProductEntity_parent_id",
                table: "ProductRelation",
                column: "parent_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductWebsite_ProductEntity_product_id",
                table: "ProductWebsite",
                column: "product_id",
                principalTable: "ProductEntity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryEntity_CategoryEntity_parent_id",
                table: "CategoryEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_EavAttribute_EavAttributeGroup_attribute_group_id",
                table: "EavAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_EavAttributeGroup_EavAttributeSet_attribute_set_id",
                table: "EavAttributeGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategory_CategoryEntity_category_id",
                table: "ProductCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategory_ProductEntity_product_id",
                table: "ProductCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntity_EavAttributeSet_attribute_set_id",
                table: "ProductEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntityDatetime_EavAttribute_attribute_id",
                table: "ProductEntityDatetime");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntityDatetime_ProductEntity_entity_id",
                table: "ProductEntityDatetime");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntityDecimal_EavAttribute_attribute_id",
                table: "ProductEntityDecimal");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntityDecimal_ProductEntity_entity_id",
                table: "ProductEntityDecimal");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntityInt_EavAttribute_attribute_id",
                table: "ProductEntityInt");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntityInt_ProductEntity_entity_id",
                table: "ProductEntityInt");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntityText_EavAttribute_attribute_id",
                table: "ProductEntityText");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntityText_ProductEntity_entity_id",
                table: "ProductEntityText");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntityVarchar_EavAttribute_attribute_id",
                table: "ProductEntityVarchar");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntityVarchar_ProductEntity_entity_id",
                table: "ProductEntityVarchar");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductInventory_ProductEntity_product_id",
                table: "ProductInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductMediaGallery_EavAttribute_attribute_id",
                table: "ProductMediaGallery");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductMediaGallery_ProductEntity_product_id",
                table: "ProductMediaGallery");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPrice_ProductEntity_product_id",
                table: "ProductPrice");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductRelation_ProductEntity_child_id",
                table: "ProductRelation");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductRelation_ProductEntity_parent_id",
                table: "ProductRelation");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductWebsite_ProductEntity_product_id",
                table: "ProductWebsite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductWebsite",
                table: "ProductWebsite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductRelation",
                table: "ProductRelation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductPrice",
                table: "ProductPrice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductMediaGallery",
                table: "ProductMediaGallery");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductInventory",
                table: "ProductInventory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductEntityVarchar",
                table: "ProductEntityVarchar");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductEntityText",
                table: "ProductEntityText");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductEntityInt",
                table: "ProductEntityInt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductEntityDecimal",
                table: "ProductEntityDecimal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductEntityDatetime",
                table: "ProductEntityDatetime");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductEntity",
                table: "ProductEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductCategory",
                table: "ProductCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EavAttributeSet",
                table: "EavAttributeSet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EavAttributeGroup",
                table: "EavAttributeGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EavAttribute",
                table: "EavAttribute");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryEntity",
                table: "CategoryEntity");

            migrationBuilder.RenameTable(
                name: "ProductWebsite",
                newName: "product_website");

            migrationBuilder.RenameTable(
                name: "ProductRelation",
                newName: "product_relation");

            migrationBuilder.RenameTable(
                name: "ProductPrice",
                newName: "product_price");

            migrationBuilder.RenameTable(
                name: "ProductMediaGallery",
                newName: "product_media_gallery");

            migrationBuilder.RenameTable(
                name: "ProductInventory",
                newName: "product_inventory");

            migrationBuilder.RenameTable(
                name: "ProductEntityVarchar",
                newName: "product_entity_varchar");

            migrationBuilder.RenameTable(
                name: "ProductEntityText",
                newName: "product_entity_text");

            migrationBuilder.RenameTable(
                name: "ProductEntityInt",
                newName: "product_entity_int");

            migrationBuilder.RenameTable(
                name: "ProductEntityDecimal",
                newName: "product_entity_decimal");

            migrationBuilder.RenameTable(
                name: "ProductEntityDatetime",
                newName: "product_entity_datetime");

            migrationBuilder.RenameTable(
                name: "ProductEntity",
                newName: "product_entity");

            migrationBuilder.RenameTable(
                name: "ProductCategory",
                newName: "product_category");

            migrationBuilder.RenameTable(
                name: "EavAttributeSet",
                newName: "eav_attribute_set");

            migrationBuilder.RenameTable(
                name: "EavAttributeGroup",
                newName: "eav_attribute_group");

            migrationBuilder.RenameTable(
                name: "EavAttribute",
                newName: "eav_attribute");

            migrationBuilder.RenameTable(
                name: "CategoryEntity",
                newName: "category_entity");

            migrationBuilder.RenameIndex(
                name: "IX_ProductRelation_parent_id_child_id_relation_type",
                table: "product_relation",
                newName: "IX_product_relation_parent_id_child_id_relation_type");

            migrationBuilder.RenameIndex(
                name: "IX_ProductRelation_child_id",
                table: "product_relation",
                newName: "IX_product_relation_child_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPrice_product_id_customer_group_id_website_id_qty",
                table: "product_price",
                newName: "IX_product_price_product_id_customer_group_id_website_id_qty");

            migrationBuilder.RenameIndex(
                name: "IX_ProductMediaGallery_product_id",
                table: "product_media_gallery",
                newName: "IX_product_media_gallery_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductMediaGallery_attribute_id",
                table: "product_media_gallery",
                newName: "IX_product_media_gallery_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntityVarchar_entity_id_attribute_id_store_id",
                table: "product_entity_varchar",
                newName: "IX_product_entity_varchar_entity_id_attribute_id_store_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntityVarchar_attribute_id",
                table: "product_entity_varchar",
                newName: "IX_product_entity_varchar_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntityText_entity_id_attribute_id_store_id",
                table: "product_entity_text",
                newName: "IX_product_entity_text_entity_id_attribute_id_store_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntityText_attribute_id",
                table: "product_entity_text",
                newName: "IX_product_entity_text_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntityInt_entity_id_attribute_id_store_id",
                table: "product_entity_int",
                newName: "IX_product_entity_int_entity_id_attribute_id_store_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntityInt_attribute_id",
                table: "product_entity_int",
                newName: "IX_product_entity_int_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntityDecimal_entity_id_attribute_id_store_id",
                table: "product_entity_decimal",
                newName: "IX_product_entity_decimal_entity_id_attribute_id_store_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntityDecimal_attribute_id",
                table: "product_entity_decimal",
                newName: "IX_product_entity_decimal_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntityDatetime_entity_id_attribute_id_store_id",
                table: "product_entity_datetime",
                newName: "IX_product_entity_datetime_entity_id_attribute_id_store_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntityDatetime_attribute_id",
                table: "product_entity_datetime",
                newName: "IX_product_entity_datetime_attribute_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntity_sku",
                table: "product_entity",
                newName: "IX_product_entity_sku");

            migrationBuilder.RenameIndex(
                name: "IX_ProductEntity_attribute_set_id",
                table: "product_entity",
                newName: "IX_product_entity_attribute_set_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductCategory_category_id",
                table: "product_category",
                newName: "IX_product_category_category_id");

            migrationBuilder.RenameIndex(
                name: "IX_EavAttributeGroup_attribute_set_id",
                table: "eav_attribute_group",
                newName: "IX_eav_attribute_group_attribute_set_id");

            migrationBuilder.RenameIndex(
                name: "IX_EavAttribute_entity_type_id_attribute_code",
                table: "eav_attribute",
                newName: "IX_eav_attribute_entity_type_id_attribute_code");

            migrationBuilder.RenameIndex(
                name: "IX_EavAttribute_attribute_group_id",
                table: "eav_attribute",
                newName: "IX_eav_attribute_attribute_group_id");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryEntity_parent_id",
                table: "category_entity",
                newName: "IX_category_entity_parent_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_website",
                table: "product_website",
                columns: new[] { "product_id", "website_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_relation",
                table: "product_relation",
                column: "relation_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_price",
                table: "product_price",
                column: "price_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_media_gallery",
                table: "product_media_gallery",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_inventory",
                table: "product_inventory",
                column: "product_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_entity_varchar",
                table: "product_entity_varchar",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_entity_text",
                table: "product_entity_text",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_entity_int",
                table: "product_entity_int",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_entity_decimal",
                table: "product_entity_decimal",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_entity_datetime",
                table: "product_entity_datetime",
                column: "value_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_entity",
                table: "product_entity",
                column: "entity_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_category",
                table: "product_category",
                columns: new[] { "product_id", "category_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_eav_attribute_set",
                table: "eav_attribute_set",
                column: "attribute_set_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_eav_attribute_group",
                table: "eav_attribute_group",
                column: "attribute_group_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_eav_attribute",
                table: "eav_attribute",
                column: "attribute_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_category_entity",
                table: "category_entity",
                column: "entity_id");

            migrationBuilder.AddForeignKey(
                name: "FK_category_entity_category_entity_parent_id",
                table: "category_entity",
                column: "parent_id",
                principalTable: "category_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_eav_attribute_eav_attribute_group_attribute_group_id",
                table: "eav_attribute",
                column: "attribute_group_id",
                principalTable: "eav_attribute_group",
                principalColumn: "attribute_group_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_eav_attribute_group_eav_attribute_set_attribute_set_id",
                table: "eav_attribute_group",
                column: "attribute_set_id",
                principalTable: "eav_attribute_set",
                principalColumn: "attribute_set_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_category_category_entity_category_id",
                table: "product_category",
                column: "category_id",
                principalTable: "category_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_category_product_entity_product_id",
                table: "product_category",
                column: "product_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_entity_eav_attribute_set_attribute_set_id",
                table: "product_entity",
                column: "attribute_set_id",
                principalTable: "eav_attribute_set",
                principalColumn: "attribute_set_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_entity_datetime_eav_attribute_attribute_id",
                table: "product_entity_datetime",
                column: "attribute_id",
                principalTable: "eav_attribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_entity_datetime_product_entity_entity_id",
                table: "product_entity_datetime",
                column: "entity_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_entity_decimal_eav_attribute_attribute_id",
                table: "product_entity_decimal",
                column: "attribute_id",
                principalTable: "eav_attribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_entity_decimal_product_entity_entity_id",
                table: "product_entity_decimal",
                column: "entity_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_entity_int_eav_attribute_attribute_id",
                table: "product_entity_int",
                column: "attribute_id",
                principalTable: "eav_attribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_entity_int_product_entity_entity_id",
                table: "product_entity_int",
                column: "entity_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_entity_text_eav_attribute_attribute_id",
                table: "product_entity_text",
                column: "attribute_id",
                principalTable: "eav_attribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_entity_text_product_entity_entity_id",
                table: "product_entity_text",
                column: "entity_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_entity_varchar_eav_attribute_attribute_id",
                table: "product_entity_varchar",
                column: "attribute_id",
                principalTable: "eav_attribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_entity_varchar_product_entity_entity_id",
                table: "product_entity_varchar",
                column: "entity_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_inventory_product_entity_product_id",
                table: "product_inventory",
                column: "product_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_media_gallery_eav_attribute_attribute_id",
                table: "product_media_gallery",
                column: "attribute_id",
                principalTable: "eav_attribute",
                principalColumn: "attribute_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_media_gallery_product_entity_product_id",
                table: "product_media_gallery",
                column: "product_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_price_product_entity_product_id",
                table: "product_price",
                column: "product_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_relation_product_entity_child_id",
                table: "product_relation",
                column: "child_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_relation_product_entity_parent_id",
                table: "product_relation",
                column: "parent_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_website_product_entity_product_id",
                table: "product_website",
                column: "product_id",
                principalTable: "product_entity",
                principalColumn: "entity_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
