UPDATE {{TableName}}
SET Barcode = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
    Barcode,
    '\', '{{ReplacementChar}}'),    -- backslash
    '/', '{{ReplacementChar}}'),    -- forward slash
    ':', '{{ReplacementChar}}'),    -- colon
    '*', '{{ReplacementChar}}'),    -- asterisk
    '?', '{{ReplacementChar}}'),    -- question mark
    '"', '{{ReplacementChar}}'),    -- double quote
    '<', '{{ReplacementChar}}'),    -- less than
    '>', '{{ReplacementChar}}'),    -- greater than
    '|', '{{ReplacementChar}}'      -- pipe
);
