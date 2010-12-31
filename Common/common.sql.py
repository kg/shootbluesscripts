from shootblues.common import log, remoteCall

def createTable(name, tableDef):
    return remoteCall("Common.Script.dll", "CreateDBTable", name, tableDef)

def query(sql, *args):
    return remoteCall("Common.Script.dll", "ExecuteSQL", sql, *args)

