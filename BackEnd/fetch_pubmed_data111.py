import argparse
import time
from bs4 import BeautifulSoup
import pandas as pd
import random
import requests
import asyncio
import aiohttp
import socket

import warnings; warnings.filterwarnings('ignore') # aiohttp produces deprecation warnings that don't concern us
# import nest_asyncio; nest_asyncio.apply() # necessary to run nested async loops in jupyter notebooks

class PubMedScraper:
    def __init__(self, pages=3, start=2020, stop=2024, output='data/papers.csv', keywords='dementia'):
        self.pages = pages
        self.start_time = time.time()
        self.start = start
        self.stop = stop
        self.output = output
        self.keywords = keywords
        self.user_agents = [
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/22.0.1207.1 Safari/537.1",
            "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/22.0.1207.1 Safari/537.1",
            "Mozilla/5.0 (X11; CrOS i686 2268.111.0) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.57 Safari/536.11",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.6 (KHTML, like Gecko) Chrome/20.0.1092.0 Safari/536.6",
            "Mozilla/5.0 (Windows NT 6.0) AppleWebKit/536.5 (KHTML, like Gecko) Chrome/19.0.1084.36 Safari/536.5",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.3 (KHTML, like Gecko) Chrome/19.0.1063.0 Safari/536.3",
            "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/536.3 (KHTML, like Gecko) Chrome/19.0.1063.0 Safari/536.3",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_0) AppleWebKit/536.3 (KHTML, like Gecko) Chrome/19.0.1063.0 Safari/536.3",
            "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/536.3 (KHTML, like Gecko) Chrome/19.0.1062.0 Safari/536.3",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.3 (KHTML, like Gecko) Chrome/19.0.1062.0 Safari/536.3",
            "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/536.3 (KHTML, like Gecko) Chrome/19.0.1061.1 Safari/536.3",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.3 (KHTML, like Gecko) Chrome/19.0.1061.1 Safari/536.3",
            "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/536.3 (KHTML, like Gecko) Chrome/19.0.1061.1 Safari/536.3",
            "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/536.6 (KHTML, like Gecko) Chrome/20.0.1090.0 Safari/536.6",
            "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/19.77.34.5 Safari/537.1",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/536.5 (KHTML, like Gecko) Chrome/19.0.1084.9 Safari/536.5",
            "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/536.3 (KHTML, like Gecko) Chrome/19.0.1061.0 Safari/536.3",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/535.24 (KHTML, like Gecko) Chrome/19.0.1055.1 Safari/535.24",
            "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/535.24 (KHTML, like Gecko) Chrome/19.0.1055.1 Safari/535.24"
        ]
        self.pubmed_url = f'https://pubmed.ncbi.nlm.nih.gov/?term={self.start}%3A{self.stop}%5Bdp%5D'
        self.root_pubmed_url = 'https://pubmed.ncbi.nlm.nih.gov'
        self.articles_data = []
        self.urls = []
        self.scraped_urls = []
        self.semaphore = asyncio.BoundedSemaphore(100)

    def make_header(self):
        headers = {
            'User-Agent': random.choice(self.user_agents),
        }
        return headers

    async def extract_by_article(self, url):
        conn = aiohttp.TCPConnector(family=socket.AF_INET)
        headers = self.make_header()
        global articles_data
        async with aiohttp.ClientSession(headers=headers, connector=conn) as session:
            async with self.semaphore, session.get(url) as response:
                data = await response.text()
                soup = BeautifulSoup(data)
                try:
                    abstract_raw = soup.find('div', {'class': 'abstract-content selected'}).find_all('p')
                    abstract = ' '.join([paragraph.text.strip() for paragraph in abstract_raw])
                except:
                    abstract = 'NO_ABSTRACT'
                affiliations = []
                try:
                    all_affiliations = soup.find('ul', {'class':'item-list'}).find_all('li')
                    for affiliation in all_affiliations:
                        affiliations.append(affiliation.get_text().strip())
                except:
                    affiliations = 'NO_AFFILIATIONS'
                try:
                    has_keywords = soup.find_all('strong',{'class':'sub-title'})[-1].text.strip()
                    if has_keywords == 'Keywords:':
                        keywords = soup.find('div', {'class':'abstract' }).find_all('p')[-1].get_text()
                        keywords = keywords.replace('Keywords:','\n').strip()
                    else:
                        keywords = 'NO_KEYWORDS'
                except:
                    keywords = 'NO_KEYWORDS'
                try:
                    title = soup.find('meta',{'name':'citation_title'})['content'].strip('[]')
                except:
                    title = 'NO_TITLE'
                authors = ''
                try:
                    for author in soup.find('div',{'class':'authors-list'}).find_all('a',{'class':'full-name'}):
                        authors += author.text + ', '
                except:
                    authors = ('NO_AUTHOR')
                try:
                    journal = soup.find('meta',{'name':'citation_journal_title'})['content']
                except:
                    journal = 'NO_JOURNAL'
                try:
                    date = soup.find('time', {'class': 'citation-year'}).text
                except:
                    date = 'NO_DATE'

                article_data = {
                    'url': url,
                    'title': title,
                    'authors': authors,
                    'abstract': abstract,
                    'affiliations': affiliations,
                    'journal': journal,
                    'keywords': keywords,
                    'date': date
                }
                self.articles_data.append(article_data)

    async def get_pmids(self, page, keyword):
        page_url = f'{self.pubmed_url}+{keyword}+&page={page}'
        headers = self.make_header()
        async with aiohttp.ClientSession(headers=headers) as session:
            async with session.get(page_url) as response:
                data = await response.text()
                soup = BeautifulSoup(data)
                pmids = soup.find('meta',{'name':'log_displayeduids'})['content']
                for pmid in pmids.split(','):
                    url = self.root_pubmed_url + '/' + pmid
                    self.urls.append(url)

    def get_num_pages(self, keyword):
        if self.pages != None:
            return self.pages
        headers = self.make_header()
        search_url = f'{self.pubmed_url}+{keyword}'
        with requests.get(search_url,headers=headers) as response:
            data = response.text
            soup = BeautifulSoup(data)
            num_pages = int((soup.find('span', {'class': 'total-pages'}).get_text()).replace(',',''))
            return num_pages

    async def build_article_urls(self, keywords):
        tasks = []
        for keyword in keywords:
            num_pages = self.get_num_pages(keyword)
            for page in range(1,num_pages+1):
                task = asyncio.create_task(self.get_pmids(page, keyword))
                tasks.append(task)

        await asyncio.gather(*tasks)

    async def get_article_data(self, urls):
        tasks = []
        for url in urls:
            if url not in self.scraped_urls:
                task = asyncio.create_task(self.extract_by_article(url))
                tasks.append(task)
                self.scraped_urls.append(url)

        await asyncio.gather(*tasks)

    def main(self):
        search_keywords = []
        for keyword in self.keywords.split('\n'):
            search_keywords.append(keyword)

        print(f'\nFinding PubMed article URLs for {len(search_keywords)} keyword sets.')

        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)

        loop = asyncio.get_event_loop()
        loop.run_until_complete(self.build_article_urls(search_keywords))
        print(f'Scraping initiated for {len(self.urls)} article URLs found from {self.start} to {self.stop}\n')

        loop = asyncio.get_event_loop()
        loop.run_until_complete(self.get_article_data(self.urls))

        articles_df = pd.DataFrame(self.articles_data, columns=['title','abstract','affiliations','authors','journal','date','keywords','url'])

        filename = self.output
        articles_df.to_csv(filename,index=False)
        print(f'It took {time.time() - self.start_time} seconds to find {len(self.urls)} articles; {len(self.scraped_urls)} unique articles were saved to {filename}')

