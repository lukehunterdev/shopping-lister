create table SlInfo
(
    Name     TEXT    not null
        constraint SlInfo_PK
            primary key,
    Value    TEXT
);

INSERT INTO SlInfo VALUES ('DbVersion', '2');

create table User
(
    Id    integer not null
        constraint User_PK
            primary key autoincrement,
    Username TEXT not null UNIQUE,
    Name  TEXT    not null,
    Email TEXT    not null UNIQUE,
    Api         TINYINT     DEFAULT(FALSE) not null,
    Verified    TINYINT     DEFAULT(FALSE) not null,
    Banned      TINYINT     DEFAULT(FALSE) not null
);

create table Password
(
    UserId integer not null
        constraint Password_PK
            primary key,
    Salt   TEXT    not null,
    Hash   TEXT    not null
);


create table List
(
    Id integer not null
        constraint List_PK
            primary key,
    UserId  integer not null,
    Name    TEXT    not null
);

CREATE TABLE Item
(
    Id integer not null
        constraint Item_PK
            primary key,
    ListId  integer not null,
    Name    TEXT    not null,
    Usage   integer default 0 not null
);

CREATE TABLE ListShare
(
    ListId  INTEGER NOT NULL,
    UserId  INTEGER NOT NULL,
    UNIQUE(ListId, UserId) ON CONFLICT IGNORE
);

CREATE TABLE WorkingListItem
(
    ListId  INTEGER NOT NULL,
    ItemId  INTEGER NOT NULL,
    Ordinal INTEGER NOT NULL,
    Ticked  TINYINT NOT NULL,
    UNIQUE(ListId, ItemId) ON CONFLICT IGNORE
);

