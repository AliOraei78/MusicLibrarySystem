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