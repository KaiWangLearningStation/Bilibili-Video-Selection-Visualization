import requests
import os
from bs4 import BeautifulSoup
import csv

# 读取CSV文件中的ID
def read_ids_from_csv(csv_file):
    ids = []
    with open(csv_file, mode='r', encoding='utf-8') as file:
        reader = csv.reader(file)
        #next(reader)  # 跳过表头
        for row in reader:
            ids.append(row[0])  # 获取每一行的ID
    return ids

# 从哔哩哔哩搜索ID并获取第一个视频的封面图
def get_video_cover(id):
    # 哔哩哔哩的搜索URL
    search_url = f"https://search.bilibili.com/all?keyword={id}"
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
    }

    # 请求页面
    response = requests.get(search_url, headers=headers)

    if response.status_code == 200:
        soup = BeautifulSoup(response.text, 'html.parser')
        
        # 找到视频列表的父元素
        video_list = soup.find('div', {'class': 'video-list row'})
        
        if video_list:
            # 获取第一个视频的封面图
            video_thumbnail = video_list.find('img')
            
            if video_thumbnail:
                # 获取封面图的URL
                cover_url = video_thumbnail.get('src') or video_thumbnail.get('data-src')
                if cover_url:
                    # 如果URL是相对路径（以'//'开头），则添加'https:'
                    if cover_url.startswith('//'):
                        cover_url = 'https:' + cover_url
                    return cover_url
                else:
                    print(f"未找到封面图：{id}")
                    return None
            else:
                print(f"未找到封面图：{id}")
                return None
        else:
            print(f"未找到视频列表：{id}")
            return None
    else:
        print(f"请求失败：{id}")
        return None

# 下载并保存封面图
def download_cover_image(url, save_path):
    if url:
        try:
            # 发起请求下载图片
            img_data = requests.get(url).content
            with open(save_path, 'wb') as img_file:
                img_file.write(img_data)
                print(f"封面图已保存：{save_path}")
        except Exception as e:
            print(f"下载封面图失败：{e}")
    else:
        print("没有封面图可以保存。")

# 主程序
def main():
    ids = read_ids_from_csv('ID.csv')  # 从CSV文件中读取ID
    save_dir = 'covers'  # 保存封面图的文件夹

    if not os.path.exists(save_dir):
        os.makedirs(save_dir)  # 如果目录不存在则创建

    # 遍历ID并获取封面图
    for idx, id in enumerate(ids, start=1):
        print(f"正在搜索：{id}...")
        cover_url = get_video_cover(id)
        
        if cover_url:
            # 设置封面图保存路径，使用ID作为文件名并保存为PNG格式
            save_path = os.path.join(save_dir, f"{id}.png")  # 以ID命名，格式为PNG
            download_cover_image(cover_url, save_path)

# 运行主程序
if __name__ == "__main__":
    main()
