from neo4j import GraphDatabase
import pandas as pd
from tqdm import tqdm

# 创建驱动
uri = "bolt://localhost:7687"
driver = GraphDatabase.driver(uri, auth=("neo4j", "zl1424625705"))

def get_data(driver, user_ids):
    with driver.session() as session:
        query = """
        UNWIND $user_ids AS user_id
        MATCH (u:User {user_id: user_id})
        CALL apoc.path.expand(u, '>', ':User', 0, 2) YIELD path
        UNWIND nodes(path) as n
        RETURN u.user_id as user, n.name as node, [r IN relationships(path) | type(r)] as relations
        """
        result = session.run(query, {"user_ids": user_ids})
        data = [{"user": record["user"], "node": record["node"], "relations": record["relations"]} for record in tqdm(result)]
        return pd.DataFrame(data)

# 用户ID列表
user_ids = ["443e7c54072d3d65fa9e64443d158f58", "64a53e0f6997e861a1958571c52be7dc", "4f6fbae5e23ed8bc0c366d331dfe0322"]

df = get_data(driver, user_ids)

# 导出为csv
df.to_csv('output.csv', index=False)
