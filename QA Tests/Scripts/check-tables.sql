-- Check for ASP.NET Identity tables
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
  AND table_name LIKE 'AspNet%'
ORDER BY table_name;
