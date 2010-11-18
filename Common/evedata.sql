-- Load an SQLite version of the EVE database dump as 'main' and then attach evedata.db as evedata before running this SQL

DROP TABLE IF EXISTS evedata.categories;
DROP TABLE IF EXISTS evedata.groups;
DROP TABLE IF EXISTS evedata.types;

CREATE TABLE evedata.categories ( categoryID INTEGER PRIMARY KEY, name TEXT, description TEXT );
CREATE TABLE evedata.groups ( groupID INTEGER PRIMARY KEY, categoryID INTEGER, name TEXT, description TEXT );
CREATE TABLE evedata.types ( typeID INTEGER PRIMARY KEY, groupID INTEGER, categoryID INTEGER, name TEXT, description TEXT );

INSERT INTO evedata.categories (categoryID, name, description) SELECT categoryID, categoryName AS name, description FROM main.invCategories;
INSERT INTO evedata.groups (groupID, categoryID, name, description) SELECT groupID, categoryID, groupName AS name, description FROM main.invGroups;
INSERT INTO evedata.types (typeID, groupID, name, description) SELECT typeID, groupID, typeName AS name, description FROM main.invTypes;

CREATE INDEX evedata.idx_categories_name ON categories (name ASC);
CREATE INDEX evedata.idx_groups_categoryID ON groups (categoryID);
CREATE INDEX evedata.idx_groups_name ON groups (name ASC);
CREATE INDEX evedata.idx_types_groupID ON types (groupID);
CREATE INDEX evedata.idx_types_name ON types (name ASC);

UPDATE evedata.types SET categoryID = (SELECT categoryID FROM evedata.groups WHERE groups.groupID = types.groupID);

CREATE INDEX evedata.idx_types_categoryID ON types (categoryID);

ANALYZE evedata;
VACUUM evedata;