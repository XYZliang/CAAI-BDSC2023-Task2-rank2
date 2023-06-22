from py2neo import Graph, Node, Relationship
from neo4j import GraphDatabase
import orjson as json
from tqdm import tqdm

# 创建连接到Neo4j数据库的Graph对象
graph = Graph("bolt://localhost:7687", auth=("neo4j", "zl1424625705"))  # replace with your actual password
driver = GraphDatabase.driver("bolt://localhost:7687", auth=("neo4j", "zl1424625705"))  # 这里的"password"需要替换成你自己数据库的密码

# # 读取user_info.json文件，并将数据作为节点存入Neo4j
# with open('./data/user_info.json', 'rb') as f:
#     user_info_data = json.loads(f.read())
# for user_info in tqdm(user_info_data, desc="存入用户实体和属性"):
#     user_node = Node("User", user_id=user_info["user_id"], user_gender=user_info["user_gender"], user_age=user_info["user_age"], user_level=user_info["user_level"])
#     graph.create(user_node)
#
# exit()
# 读取item_info.json文件，将商品信息作为词典存储，便于后续查询
item_info_dict = {}
with open('./data/item_info.json', 'rb') as f:
    item_info_data = json.loads(f.read())
for item_info in tqdm(item_info_data, desc="获取商品实体和属性"):
    item_info_dict[item_info["item_id"]] = item_info

# 读取item_share_train_info.json文件，并将数据作为关系存入Neo4j
with open('./data/item_share_train_info_B.json', 'rb') as f:
    item_share_train_info_data = json.loads(f.read())
with driver.session() as session:
    for item_share_train_info in tqdm(item_share_train_info_data,desc="存入分享关系和属性"):
        item_info = item_info_dict[item_share_train_info["item_id"]]
        session.run(
            "MATCH (a:User {user_id: $user_id}), (b:User {user_id: $voter_id}) "
            "CREATE (a)-[:SHARE {item_id: $item_id, timestamp: $timestamp, cate_id: $cate_id, cate_level1_id: $cate_level1_id, brand_id: $brand_id, shop_id: $shop_id}]->(b)",
            user_id=item_share_train_info["inviter_id"], voter_id=item_share_train_info["voter_id"],
            item_id=item_info["item_id"], timestamp=item_share_train_info["timestamp"],
            cate_id=item_info["cate_id"], cate_level1_id=item_info["cate_level1_id"],
            brand_id=item_info["brand_id"], shop_id=item_info["shop_id"]
        )

driver.close()
