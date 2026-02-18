SELECT con.conname AS constraint_name,
       con.contype AS constraint_type,
       ARRAY_AGG(att.attname) AS columns
FROM pg_constraint con
JOIN pg_class rel ON con.conrelid = rel.oid
JOIN pg_namespace nsp ON rel.relnamespace = nsp.oid
LEFT JOIN pg_attribute att ON att.attrelid = rel.oid AND att.attnum = ANY(con.conkey)
WHERE rel.relname = 'AspNetUsers'
  AND nsp.nspname = 'public'
GROUP BY con.conname, con.contype
ORDER BY con.contype, con.conname;
