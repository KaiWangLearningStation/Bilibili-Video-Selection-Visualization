import json
import csv

# 读取JSON文件
with open('ID.json', 'r', encoding='utf-8') as file:
    data = json.load(file)

# 提取节点的'id'字段
node_ids = [node["id"] for node in data["nodes"]]

# 将数据保存到CSV文件
with open("ID.csv", "w", newline="", encoding="utf-8") as csvfile:
    writer = csv.writer(csvfile)
    writer.writerow(["id"])  # 写入表头
    for node_id in node_ids:
        writer.writerow([node_id])  # 写入每个节点的'id'

print("CSV文件已成功保存!")