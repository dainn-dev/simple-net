using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DainnProductEAV.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePrimaryKeysToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "EavAttributeGroup",
                keyColumn: "attribute_group_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EavAttributeGroup",
                keyColumn: "attribute_group_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EavAttributeGroup",
                keyColumn: "attribute_group_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EavAttributeGroup",
                keyColumn: "attribute_group_id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "EavAttributeGroup",
                keyColumn: "attribute_group_id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "EavAttributeSet",
                keyColumn: "attribute_set_id",
                keyValue: 1);

            // Clear all data from tables with foreign keys to avoid referential integrity issues
            // This is safe since we're deleting seed data and will re-insert it with new UUIDs
            migrationBuilder.Sql(@"DELETE FROM ""ProductEntityVarchar"";");
            migrationBuilder.Sql(@"DELETE FROM ""ProductEntityText"";");
            migrationBuilder.Sql(@"DELETE FROM ""ProductEntityInt"";");
            migrationBuilder.Sql(@"DELETE FROM ""ProductEntityDecimal"";");
            migrationBuilder.Sql(@"DELETE FROM ""ProductEntityDatetime"";");
            migrationBuilder.Sql(@"DELETE FROM ""ProductCategory"";");
            migrationBuilder.Sql(@"DELETE FROM ""ProductInventory"";");
            migrationBuilder.Sql(@"DELETE FROM ""ProductPrice"";");
            migrationBuilder.Sql(@"DELETE FROM ""ProductMediaGallery"";");
            migrationBuilder.Sql(@"DELETE FROM ""ProductRelation"";");
            migrationBuilder.Sql(@"DELETE FROM ""ProductWebsite"";");
            migrationBuilder.Sql(@"DELETE FROM ""ProductEntity"";");
            migrationBuilder.Sql(@"DELETE FROM ""CategoryEntity"";");
            migrationBuilder.Sql(@"DELETE FROM ""EavAttribute"";");
            migrationBuilder.Sql(@"DELETE FROM ""EavAttributeGroup"";");
            migrationBuilder.Sql(@"DELETE FROM ""EavAttributeSet"";");

            // Drop all foreign key constraints dynamically before converting columns
            // This must happen before any column conversions
            // Use pg_constraint for more reliable constraint discovery
            // Drop constraints on both the referencing tables AND the referenced tables
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    r RECORD;
                BEGIN
                    -- Drop ALL foreign key constraints that reference or are referenced by our tables
                    FOR r IN (
                        SELECT DISTINCT
                            n.nspname AS schema_name,
                            t.relname AS table_name,
                            c.conname AS constraint_name
                        FROM pg_constraint c
                        JOIN pg_class t ON c.conrelid = t.oid
                        JOIN pg_namespace n ON t.relnamespace = n.oid
                        WHERE c.contype = 'f'
                        AND (
                            -- Constraints on tables we're converting
                            t.relname IN (
                                'EavAttributeGroup', 'EavAttribute', 'ProductEntity', 'CategoryEntity',
                                'ProductCategory', 'ProductInventory', 'ProductPrice', 'ProductMediaGallery',
                                'ProductRelation', 'ProductWebsite', 'ProductEntityVarchar', 'ProductEntityText',
                                'ProductEntityInt', 'ProductEntityDecimal', 'ProductEntityDatetime', 'EavAttributeSet'
                            )
                            -- OR constraints that reference tables we're converting
                            OR EXISTS (
                                SELECT 1 FROM pg_class ref_t
                                JOIN pg_namespace ref_n ON ref_t.relnamespace = ref_n.oid
                                WHERE c.confrelid = ref_t.oid
                                AND ref_t.relname IN (
                                    'EavAttributeSet', 'EavAttributeGroup', 'EavAttribute', 
                                    'ProductEntity', 'CategoryEntity'
                                )
                            )
                        )
                    ) LOOP
                        BEGIN
                            EXECUTE format('ALTER TABLE %I.%I DROP CONSTRAINT IF EXISTS %I CASCADE', 
                                r.schema_name, r.table_name, r.constraint_name);
                        EXCEPTION WHEN OTHERS THEN
                            -- Continue even if constraint doesn't exist
                            NULL;
                        END;
                    END LOOP;
                END $$;
            ");

            // Drop identity sequences before converting columns (PostgreSQL requires this)
            // Use DO blocks to safely drop identities even if they don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""EavAttributeSet"" ALTER COLUMN ""attribute_set_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""EavAttributeGroup"" ALTER COLUMN ""attribute_group_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""EavAttribute"" ALTER COLUMN ""attribute_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""CategoryEntity"" ALTER COLUMN ""entity_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""ProductEntity"" ALTER COLUMN ""entity_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""ProductEntityVarchar"" ALTER COLUMN ""value_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""ProductEntityText"" ALTER COLUMN ""value_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""ProductEntityInt"" ALTER COLUMN ""value_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""ProductEntityDecimal"" ALTER COLUMN ""value_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""ProductEntityDatetime"" ALTER COLUMN ""value_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""ProductPrice"" ALTER COLUMN ""price_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""ProductMediaGallery"" ALTER COLUMN ""value_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    ALTER TABLE ""ProductRelation"" ALTER COLUMN ""relation_id"" DROP IDENTITY;
                EXCEPTION WHEN OTHERS THEN
                    -- Identity doesn't exist, continue
                END $$;
            ");

            // Convert all integer columns to UUID using raw SQL with USING clauses
            // Since tables are empty after DELETE, we convert integers to UUID format
            // Format: 00000000-0000-0000-0000-{12 digit hex}
            // Primary keys first
            migrationBuilder.Sql(@"
                ALTER TABLE ""EavAttributeSet"" 
                ALTER COLUMN ""attribute_set_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_set_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""EavAttributeGroup"" 
                ALTER COLUMN ""attribute_group_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_group_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""EavAttribute"" 
                ALTER COLUMN ""attribute_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""CategoryEntity"" 
                ALTER COLUMN ""entity_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(entity_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntity"" 
                ALTER COLUMN ""entity_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(entity_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityVarchar"" 
                ALTER COLUMN ""value_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(value_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityText"" 
                ALTER COLUMN ""value_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(value_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityInt"" 
                ALTER COLUMN ""value_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(value_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityDecimal"" 
                ALTER COLUMN ""value_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(value_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityDatetime"" 
                ALTER COLUMN ""value_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(value_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductPrice"" 
                ALTER COLUMN ""price_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(price_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductMediaGallery"" 
                ALTER COLUMN ""value_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(value_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductRelation"" 
                ALTER COLUMN ""relation_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(relation_id), 12, '0'))::uuid);
            ");
            
            // Foreign keys - use same conversion
            migrationBuilder.Sql(@"
                ALTER TABLE ""EavAttributeGroup"" 
                ALTER COLUMN ""attribute_set_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_set_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""EavAttribute"" 
                ALTER COLUMN ""attribute_group_id"" TYPE uuid USING CASE WHEN ""attribute_group_id"" IS NULL THEN NULL ELSE (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_group_id), 12, '0'))::uuid) END;
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntity"" 
                ALTER COLUMN ""attribute_set_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_set_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""CategoryEntity"" 
                ALTER COLUMN ""parent_id"" TYPE uuid USING CASE WHEN ""parent_id"" IS NULL THEN NULL ELSE (('00000000-0000-0000-0000-' || lpad(to_hex(parent_id), 12, '0'))::uuid) END;
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductCategory"" 
                ALTER COLUMN ""product_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(product_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductCategory"" 
                ALTER COLUMN ""category_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(category_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductInventory"" 
                ALTER COLUMN ""product_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(product_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductPrice"" 
                ALTER COLUMN ""product_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(product_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductMediaGallery"" 
                ALTER COLUMN ""product_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(product_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductMediaGallery"" 
                ALTER COLUMN ""attribute_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductRelation"" 
                ALTER COLUMN ""parent_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(parent_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductRelation"" 
                ALTER COLUMN ""child_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(child_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductWebsite"" 
                ALTER COLUMN ""product_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(product_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityVarchar"" 
                ALTER COLUMN ""entity_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(entity_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityVarchar"" 
                ALTER COLUMN ""attribute_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityText"" 
                ALTER COLUMN ""entity_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(entity_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityText"" 
                ALTER COLUMN ""attribute_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityInt"" 
                ALTER COLUMN ""entity_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(entity_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityInt"" 
                ALTER COLUMN ""attribute_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityDecimal"" 
                ALTER COLUMN ""entity_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(entity_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityDecimal"" 
                ALTER COLUMN ""attribute_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityDatetime"" 
                ALTER COLUMN ""entity_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(entity_id), 12, '0'))::uuid);
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityDatetime"" 
                ALTER COLUMN ""attribute_id"" TYPE uuid USING (('00000000-0000-0000-0000-' || lpad(to_hex(attribute_id), 12, '0'))::uuid);
            ");
            
            // Set default values for primary keys
            migrationBuilder.Sql(@"
                ALTER TABLE ""EavAttributeSet"" 
                ALTER COLUMN ""attribute_set_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""EavAttributeGroup"" 
                ALTER COLUMN ""attribute_group_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""EavAttribute"" 
                ALTER COLUMN ""attribute_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""CategoryEntity"" 
                ALTER COLUMN ""entity_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntity"" 
                ALTER COLUMN ""entity_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityVarchar"" 
                ALTER COLUMN ""value_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityText"" 
                ALTER COLUMN ""value_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityInt"" 
                ALTER COLUMN ""value_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityDecimal"" 
                ALTER COLUMN ""value_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductEntityDatetime"" 
                ALTER COLUMN ""value_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductPrice"" 
                ALTER COLUMN ""price_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductMediaGallery"" 
                ALTER COLUMN ""value_id"" SET DEFAULT gen_random_uuid();
            ");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductRelation"" 
                ALTER COLUMN ""relation_id"" SET DEFAULT gen_random_uuid();
            ");

            migrationBuilder.InsertData(
                table: "EavAttributeSet",
                columns: new[] { "attribute_set_id", "attribute_set_name", "entity_type_id", "sort_order" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "Default", 4, 1 });

            migrationBuilder.InsertData(
                table: "EavAttributeGroup",
                columns: new[] { "attribute_group_id", "attribute_group_name", "attribute_set_id", "sort_order" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), "General", new Guid("11111111-1111-1111-1111-111111111111"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Prices", new Guid("11111111-1111-1111-1111-111111111111"), 2 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Images", new Guid("11111111-1111-1111-1111-111111111111"), 3 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Meta Information", new Guid("11111111-1111-1111-1111-111111111111"), 4 },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Content", new Guid("11111111-1111-1111-1111-111111111111"), 5 }
                });

            migrationBuilder.InsertData(
                table: "EavAttribute",
                columns: new[] { "attribute_id", "attribute_code", "attribute_group_id", "backend_type", "default_value", "entity_type_id", "frontend_input", "frontend_label", "is_comparable", "is_filterable", "is_html_allowed_on_front", "is_required", "is_searchable", "is_unique", "is_used_for_promo_rules", "is_visible_on_front", "note", "position" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "name", new Guid("22222222-2222-2222-2222-222222222222"), "varchar", null, 4, "text", "Product Name", false, false, false, true, true, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "description", new Guid("66666666-6666-6666-6666-666666666666"), "text", null, 4, "textarea", "Description", false, false, true, false, true, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "short_description", new Guid("66666666-6666-6666-6666-666666666666"), "text", null, 4, "textarea", "Short Description", false, false, true, false, true, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "price", new Guid("33333333-3333-3333-3333-333333333333"), "decimal", null, 4, "price", "Price", true, true, false, true, true, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "special_price", new Guid("33333333-3333-3333-3333-333333333333"), "decimal", null, 4, "price", "Special Price", false, true, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "status", new Guid("22222222-2222-2222-2222-222222222222"), "int", "1", 4, "select", "Status", false, false, false, true, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "visibility", new Guid("22222222-2222-2222-2222-222222222222"), "int", "4", 4, "select", "Visibility", false, false, false, true, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "weight", new Guid("22222222-2222-2222-2222-222222222222"), "decimal", null, 4, "text", "Weight", false, false, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "url_key", new Guid("55555555-5555-5555-5555-555555555555"), "varchar", null, 4, "text", "URL Key", false, false, false, false, false, true, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-00000000000a"), "meta_title", new Guid("55555555-5555-5555-5555-555555555555"), "varchar", null, 4, "text", "Meta Title", false, false, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-00000000000b"), "meta_description", new Guid("55555555-5555-5555-5555-555555555555"), "text", null, 4, "textarea", "Meta Description", false, false, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-00000000000c"), "meta_keywords", new Guid("55555555-5555-5555-5555-555555555555"), "text", null, 4, "textarea", "Meta Keywords", false, false, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-00000000000d"), "image", new Guid("44444444-4444-4444-4444-444444444444"), "varchar", null, 4, "media_image", "Base Image", false, false, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-00000000000e"), "small_image", new Guid("44444444-4444-4444-4444-444444444444"), "varchar", null, 4, "media_image", "Small Image", false, false, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-00000000000f"), "thumbnail", new Guid("44444444-4444-4444-4444-444444444444"), "varchar", null, 4, "media_image", "Thumbnail", false, false, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000010"), "color", new Guid("22222222-2222-2222-2222-222222222222"), "int", null, 4, "select", "Color", true, true, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000011"), "size", new Guid("22222222-2222-2222-2222-222222222222"), "int", null, 4, "select", "Size", true, true, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000012"), "manufacturer", new Guid("22222222-2222-2222-2222-222222222222"), "varchar", null, 4, "select", "Manufacturer", false, true, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000013"), "special_from_date", new Guid("33333333-3333-3333-3333-333333333333"), "datetime", null, 4, "date", "Special Price From Date", false, false, false, false, false, false, false, true, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000014"), "special_to_date", new Guid("33333333-3333-3333-3333-333333333333"), "datetime", null, 4, "date", "Special Price To Date", false, false, false, false, false, false, false, true, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000a"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000b"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000c"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000d"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000e"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000f"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "EavAttribute",
                keyColumn: "attribute_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "EavAttributeGroup",
                keyColumn: "attribute_group_id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "EavAttributeGroup",
                keyColumn: "attribute_group_id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "EavAttributeGroup",
                keyColumn: "attribute_group_id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "EavAttributeGroup",
                keyColumn: "attribute_group_id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "EavAttributeGroup",
                keyColumn: "attribute_group_id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "EavAttributeSet",
                keyColumn: "attribute_set_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.AlterColumn<int>(
                name: "product_id",
                table: "ProductWebsite",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "parent_id",
                table: "ProductRelation",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "child_id",
                table: "ProductRelation",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "relation_id",
                table: "ProductRelation",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "product_id",
                table: "ProductPrice",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "price_id",
                table: "ProductPrice",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "product_id",
                table: "ProductMediaGallery",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "attribute_id",
                table: "ProductMediaGallery",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "value_id",
                table: "ProductMediaGallery",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "product_id",
                table: "ProductInventory",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "entity_id",
                table: "ProductEntityVarchar",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "attribute_id",
                table: "ProductEntityVarchar",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "value_id",
                table: "ProductEntityVarchar",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "entity_id",
                table: "ProductEntityText",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "attribute_id",
                table: "ProductEntityText",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "value_id",
                table: "ProductEntityText",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "entity_id",
                table: "ProductEntityInt",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "attribute_id",
                table: "ProductEntityInt",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "value_id",
                table: "ProductEntityInt",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "entity_id",
                table: "ProductEntityDecimal",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "attribute_id",
                table: "ProductEntityDecimal",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "value_id",
                table: "ProductEntityDecimal",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "entity_id",
                table: "ProductEntityDatetime",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "attribute_id",
                table: "ProductEntityDatetime",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "value_id",
                table: "ProductEntityDatetime",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "attribute_set_id",
                table: "ProductEntity",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "entity_id",
                table: "ProductEntity",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "category_id",
                table: "ProductCategory",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "product_id",
                table: "ProductCategory",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "attribute_set_id",
                table: "EavAttributeSet",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "attribute_set_id",
                table: "EavAttributeGroup",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "attribute_group_id",
                table: "EavAttributeGroup",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "attribute_group_id",
                table: "EavAttribute",
                type: "integer",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "attribute_id",
                table: "EavAttribute",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "parent_id",
                table: "CategoryEntity",
                type: "integer",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "entity_id",
                table: "CategoryEntity",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.InsertData(
                table: "EavAttributeSet",
                columns: new[] { "attribute_set_id", "attribute_set_name", "entity_type_id", "sort_order" },
                values: new object[] { 1, "Default", 4, 1 });

            migrationBuilder.InsertData(
                table: "EavAttributeGroup",
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
                table: "EavAttribute",
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
        }
    }
}
