CREATE TABLE ExternalDomainBoundaryMethod
(
    RecordId INTEGER PRIMARY KEY AUTOINCREMENT,
    ModuleName               TEXT (250),
    ExternalAssemblyFileName TEXT (500),
    ExternalClassFullName    TEXT (1000),
    ExternalMethodFullName   TEXT (1000)
);
  
CREATE TABLE ExternalDomainBoundaryProperty
(
        RecordId INTEGER PRIMARY KEY AUTOINCREMENT,
        MethodId               INTEGER,
        RelatedClassFullName     TEXT (1000),
        RelatedPropertyFullName  TEXT (1000)
);