## Day 1: Dapper Basics – Installation, Connection, First Query

**Completed Today:**
- Created new independent solution: MusicLibrarySystem
- Project structure: Api, Core (Models), Data (Dapper Repositories)
- Installed Dapper and Npgsql
- Created MusicLibraryDb database in PostgreSQL
- Implemented first simple Dapper queries: GetAll and GetById
- Created Albums table with seed data
- Built AlbumsController with Swagger endpoints
- Tested first Dapper queries successfully

**Key Learnings:**
- Dapper is a micro-ORM — fast and lightweight
- Manual SQL queries with full control
- No tracking, no change tracker — pure performance
- Perfect for read-heavy or custom query scenarios

## Day 2: Parameterized Queries, QueryAsync, QueryFirst/Single

**Completed Today:**
- Implemented parameterized queries with Dapper
- Added QueryAsync for multiple results
- QueryFirst/QueryFirstOrDefault for first record
- QuerySingle/QuerySingleOrDefault for exactly one record
- Safe string search with ILIKE and % pattern
- Endpoints for testing all query types
- Demonstrated SQL Injection prevention with parameters

**Key Learnings:**
- Always use parameters — Dapper prevents SQL Injection automatically
- QueryAsync: for lists (0 to many)
- QueryFirst*: first record or exception/null
- QuerySingle*: exactly one record or exception/null
- Use appropriate method to match expected result count
- ILIKE + % for case-insensitive partial search

## Day 3: Multi-Mapping, QueryMultiple, Buffered/Unbuffered

**Completed Today:**
- Multi-Mapping: AlbumWithTracksDto with 1-N relationship
- QueryMultiple: Multiple results in one round-trip
- QueryBuffered (default) vs QueryUnbuffered (streaming)
- Endpoints for testing all advanced query features
- Manual dictionary for de-duplication in multi-mapping
- Streaming processing for large datasets

**Key Learnings:**
- Buffered: Fast for small/medium data, loads everything in memory
- Unbuffered: Low memory for large datasets, streaming processing
- Use Unbuffered when dealing with millions of rows
- QueryMultiple reduces network round-trips

## Day 4: Execute, ExecuteAsync, Stored Procedures

**Completed Today:**
- CRUD operations with Execute/ExecuteAsync (Insert/Update/Delete)
- Returning new ID with ExecuteScalarAsync
- Calling Stored Procedure with parameters
- Endpoints for full CRUD and SP testing
- Rows affected handling for success/failure

**Key Learnings:**
- Execute/ExecuteAsync for non-query operations (INSERT/UPDATE/DELETE)
- ExecuteScalarAsync for returning single value (e.g., new ID)
- Stored Procedures with Dapper using CALL syntax
- Always use parameters for security
- Rows affected for checking operation success

## Day 5: Transactions with Dapper

**Completed Today:**
- Simple Dapper Transaction (BeginTransaction / Commit / Rollback)
- TransactionScope for distributed transactions
- Insert album + tracks in one transaction
- Endpoints for testing transactional operations
- Full rollback on failure

**Key Learnings:**
- Dapper Transaction: Manual control with Begin/Commit/Rollback
- TransactionScope: Automatic distributed transactions
- Always wrap in try-catch for rollback
- Perfect for multi-step operations (e.g., order + payment)

## Day 6: Performance Tuning – Caching & Batch Execute

**Completed Today:**
- Batch Insert/Update/Delete for large datasets (10,000+ records)
- Manual caching with IMemoryCache
- Query caching with expiration
- Performance comparison (cached vs uncached)
- Endpoints for testing batch and cache

**Key Learnings:**
- Batch operations are essential for large data (milliseconds vs seconds)
- Caching reduces database hits for repeated queries
- Use IMemoryCache for simple in-memory caching
- Measure performance with Stopwatch for real benchmarks

## Day 7: Dapper.Contrib – Auto CRUD

**Completed Today:**
- Installed Dapper.Contrib
- Used [Table] and [Key] attributes for automatic mapping
- Implemented Auto CRUD: GetAll, GetById, Insert, Update, Delete
- No manual SQL needed for basic operations
- Endpoints for testing Auto CRUD
- Compared with raw Dapper (less code, same performance)

**Key Learnings:**
- Dapper.Contrib simplifies CRUD with attributes
- [Key] for primary key, [Table] for table name
- Insert returns new ID automatically
- Update/Delete need entity with Id set
- Perfect for simple entities (combine with raw Dapper for complex queries)

Note: Below query is required in order to test day 7 changes:
-- Rename columns to lowercase so Dapper.Contrib can find them
ALTER TABLE "Albums" RENAME COLUMN "Id" TO id;
ALTER TABLE "Albums" RENAME COLUMN "Title" TO title;
ALTER TABLE "Albums" RENAME COLUMN "Artist" TO artist;
ALTER TABLE "Albums" RENAME COLUMN "Year" TO year;
ALTER TABLE "Albums" RENAME COLUMN "Rating" TO rating;

-- Do the same for Tracks if you want Auto-CRUD there too
ALTER TABLE "Tracks" RENAME COLUMN "Id" TO id;
ALTER TABLE "Tracks" RENAME COLUMN "Title" TO title;
ALTER TABLE "Tracks" RENAME COLUMN "DurationSeconds" TO duration_seconds;
ALTER TABLE "Tracks" RENAME COLUMN "AlbumId" TO album_id;

## Day 8: Dapper.AmbientContext & Multi-Connection

**Completed Today:**
- Implemented Ambient Connection Context (no manual connection per method)
- Scoped connection lifetime in ASP.NET Core
- Multi-database support with separate connection providers
- Safe async/await usage with Ambient context
- Endpoints for testing multi-db queries

**Key Learnings:**
- AmbientContext eliminates repetitive connection code
- Scoped services for connection lifetime management
- Multi-connection for reporting, sharding, or legacy DBs
- No connection leaks with proper Dispose
- Thread-safe in async environments

Note: Back up "MusicLibraryDb" database and restore as "MusicLibraryReportsDb" in order to test "multi-db-test" endpoint.

## Day 9: Hybrid Approach – Dapper + EF Core

**Completed Today:**
- Registered both EF Core and Dapper in DI
- Created Hybrid Repository using EF for complex relations and Dapper for fast reports
- Implemented EF method with Include for 1-N loading
- Implemented Dapper method for aggregate report (COUNT, AVG)
- Endpoints for testing hybrid approach
- Compared use cases: EF for ORM full, Dapper for performance-critical

**Key Learnings:**
- Hybrid = EF for write/complex + Dapper for read-heavy/custom
- Separate contexts/repositories for clean separation
- Use Dapper for reports, Batch, raw SQL
- Use EF for validation, relations, migrations
- No conflict — both can use same DB