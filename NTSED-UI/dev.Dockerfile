FROM node:16 as react-build
VOLUME [ "/app" ]
WORKDIR /app
COPY . .
RUN npm install
EXPOSE 1946
CMD ["npm", "run", "start"]