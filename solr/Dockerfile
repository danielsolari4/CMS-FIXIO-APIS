FROM solr:8.11.4

COPY /solr_home /opt/solr/server/solr
COPY /mssql-jdbc-9.4.1.jre8.jar /opt/solr/contrib/dataimporthandler/lib/mssql-jdbc.jar

USER root
RUN chown -R solr:solr /opt/solr/server/solr && chown solr:solr /opt/solr/contrib/dataimporthandler/lib/mssql-jdbc.jar
USER solr