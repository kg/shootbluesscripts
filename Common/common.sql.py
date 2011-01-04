from shootblues.common import log, remoteCall

def createTable(name, tableDef):
    return remoteCall("Common.Script.dll", "CreateDBTable", name, tableDef, async=True)

def query(sql, *args):
    return remoteCall("Common.Script.dll", "ExecuteSQL", sql, *args, async=True)