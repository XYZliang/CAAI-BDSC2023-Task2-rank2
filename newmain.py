# 首先，我们导入需要使用的库
from collections import defaultdict
from sortedcontainers import SortedDict
import orjson
from tqdm import tqdm
from tkinter import filedialog
from datetime import datetime


# 创建一个用于选择文件的对话框
def select_file(title):
    filedialog.Title = title
    return filedialog.askopenfilename()


# 对于四个不同的数据文件，我们分别进行选择和读取操作
# 首先是分享数据文件
share_data_file = select_file("分享数据")
# 使用orjson库读取并解析json文件
with open(share_data_file, 'r') as f:
    share_data = orjson.loads(f.read())

# 接着是商品数据文件
item_data_file = select_file("商品数据")
with open(item_data_file, 'r') as f:
    item_data = orjson.loads(f.read())

# 然后是用户数据文件
user_data_file = select_file("用户数据")
with open(user_data_file, 'r') as f:
    user_data = orjson.loads(f.read())

# 最后是测试数据文件
test_data_file = select_file("测试数据")
with open(test_data_file, 'r') as f:
    test_data = orjson.loads(f.read())

# 初始化两个按时间排序的字典，用来存储不同的链接信息
data = SortedDict()
data2 = SortedDict()

# 初始化几个变量
dt = None
lk = None
n = 0
k = 0
itemid = ""
id1 = ""
id2 = ""
