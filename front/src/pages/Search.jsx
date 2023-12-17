import React, { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { useQuery } from 'react-query';
import { PubService } from '../app/services/api';
import data from '../app/store/data.json';
import Filter from '../components/Home/Filter';
import PublicationList from '../components/Publications/PublicationList';
import Background from '../components/UI/Background';
import Pagination from '../components/UI/Pagination';
import Searchbar from '../components/UI/Searchbar';
import * as Styled from '../styles/Results.styled';
import { Main } from '../styles/UI.styled';

const SearchPage = () => {
  const [jsonData, setJsonData] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(2);
  const [searchParams, setSearchParams] = useSearchParams();
  const { refetchByAuthor } = useQuery(
    'getPubsByAuthor',
    (payload) => PubService.getPubsByAuthor(payload),
    {
      onError: (error) => {
        console.log('Get Publications By Authors error: ' + error.message);
      },
      onSuccess: (data) => {
        setJsonData(data);
      },
      enabled: false,
    },
  );

  const { refetchByKeyword } = useQuery(
    'getPubsByKeyword',
    (payload) => PubService.getPubsByKeyword(payload),
    {
      onError: (error) => {
        console.log('Get Publications By Keyword error: ' + error.message);
      },
      onSuccess: (data) => {
        setJsonData(data);
      },
      enabled: false,
    },
  );
  const postsPerPage = 4;

  useEffect(() => {
    setJsonData(data.data);
    const pageValue = parseInt(searchParams.get('page'));
    if (pageValue) {
      setCurrentPage(pageValue);
    }
  }, []);

  useEffect(() => {
    setTotalPages(Math.ceil(jsonData.length / postsPerPage));
  }, [jsonData]);

  useEffect(() => {
    setSearchParams({ page: currentPage });
  }, [currentPage]);

  const handleSubmit = (payload) => {
    const request = {
      Query: payload.query,
      Language: 'eng',
    };

    payload.type ? refetchByKeyword(request) : refetchByAuthor(request);
  };

  const lastPostIndex = currentPage * postsPerPage;
  const firstPostIndex = lastPostIndex - postsPerPage;
  const currentPosts = jsonData.slice(firstPostIndex, lastPostIndex);
  return (
    <Main>
      <Styled.MainSearchbar>
        <Background />
        <Searchbar handleSubmit={handleSubmit} />
      </Styled.MainSearchbar>
      <Styled.MainWrapper>
        <Styled.MainContent>
          <Styled.FoundHeader>
            <h2>Your Search Result</h2>
            <Pagination
              totalPages={totalPages}
              currentPage={currentPage}
              setCurrentPage={setCurrentPage}
            />
          </Styled.FoundHeader>
          <Styled.FoundContent>
            <Filter />
            <div className='divider'></div>
            {jsonData.length === 0 ? (
              <div>Loading...</div>
            ) : (
              <PublicationList data={currentPosts} />
            )}
          </Styled.FoundContent>
        </Styled.MainContent>
      </Styled.MainWrapper>
    </Main>
  );
};

export default SearchPage;
