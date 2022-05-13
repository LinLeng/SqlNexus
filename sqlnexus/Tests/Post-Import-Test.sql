﻿--This script can be used to validate that tables exist 
--Not all tables will exist in every import because they depend on what data was collected by tools like PSSDIAG or SQLLogScout
--But this is a base to start with 

select top 5 * from tbl_IMPORTEDFILES
select top 5 * from tbl_SCRIPT_ENVIRONMENT_DETAILS
select top 5 * from tbl_SYSPERFINFO
select top 5 * from tbl_SYSPERFINFO
select top 5 * from tbl_SQL_CPU_HEALTH													
select top 5 * from tbl_RUNTIMES	
select top 5 * from tbl_SPINLOCKSTATS
select top 5 * from tbl_MEMORYSTATUS_BUF_DISTRIBUTION									
select top 5 * from tbl_MEMORYSTATUS_BUF_COUNTS
select top 5 * from tbl_MEMORYSTATUS_PROC_CACHE
select top 5 * from tbl_MEMORYSTATUS_DYNAMIC_MEM_MGR
select top 5 * from tbl_MEMORYSTATUS_GLOBAL_MEM_OBJ
select top 5 * from tbl_MEMORYSTATUS_QUERY_MEM_OBJ
select top 5 * from tbl_MEMORYSTATUS_OPTIMIZATION_QUEUE
 select top 5 * from tbl_ERRORLOGS_RAW
 select top 5 * from tbl_SPCONFIGURE
select top 5 * from tbl_loaded_modules
select top 5 * from tbl_dm_os_loaded_modules_non_microsoft
select top 5 * from tbl_dm_os_loaded_modules
select top 5 * from tbl_Query_Execution_Memory
select top 5 * from tbl_Query_Execution_Memory_MemScript
select top 5 * from tbl_dm_os_ring_buffers_mem
select top 5 * from tbl_dm_os_memory_objects
select top 5 * from tbl_dm_os_memory_pools
select top 5 * from tbl_sysaltfiles
select top 5 * from tbl_StartupParameters
select top 5 * from tbl_SPHELPDB
select top 5 * from tbl_XPMSVER
select top 5 * from tbl_REQUESTS
select top 5 * from tbl_NOTABLEACTIVEQUERIES
select top 5 * from tbl_HEADBLOCKERSUMMARY
select top 5 * from tbl_OS_WAIT_STATS
select top 5 * from tbl_SYSOBJECTS
select top 5 * from tbl_DM_OS_MEMORY_CLERKS
select top 5 * from tbl_DM_OS_MEMORY_CLERKS
select top 5 * from tbl_DM_OS_MEMORY_CACHE_COUNTERS
select top 5 * from tbl_DM_OS_MEMORY_CACHE_COUNTERS
select top 5 * from tbl_DM_OS_MEMORY_CACHE_CLOCK_HANDS
select top 5 * from tbl_DM_OS_MEMORY_CACHE_CLOCK_HANDS
select top 5 * from tbl_DM_OS_MEMORY_CACHE_CLOCK_HANDS
select top 5 * from tbl_DM_OS_MEMORY_CACHE_HASH_TABLES
select top 5 * from tbl_DM_OS_MEMORY_CACHE_HASH_TABLES
select top 5 * from tblErrorlog
select top 5 * from tbl_FileStats
select top 5 * from tbl_DM_OS_MEMORY_CACHE_ENTRIES
select top 5 * from tbl_DM_OS_MEMORY_CACHE_ENTRIES
select top 5 * from tbl_DM_EXEC_CACHED_PLANS
select top 5 * from tbl_DM_EXEC_QUERY_STATS
select top 5 * from tbl_DM_OS_MEMORY_OBJECTS
select top 5 * from tbl_DM_EXEC_CONNECTIONS
select top 5 * from tbl_MissingIndexes
select top 5 * from tbl_dm_db_stats_properties_for_master
select top 5 * from tbl_SYSINDEXES
select top 5 * from tbl_OS_WAIT_STATS
select top 5 * from tbl_RESOURCE_STATS
select top 5 * from tbl_RESOURCE_USAGE
select top 5 * from tbl_DB_CONN_STATS
select top 5 * from tbl_EVENT_LOG
select top 5 * from tbl_sp_configure
select top 5 * from tbl_dm_os_memory_brokers
select top 5 * from tbl_dm_exec_query_memory_grants
select top 5 * from tbl_dm_exec_query_resource_semaphores
select top 5 * from tbl_TopN_QueryPlanStats
select top 5 * from tbl_workingset_trimming
select top 5 * from tbl_PowerPlan
select top 5 * from tbl_ThreadStats
select top 5 * from tbl_LockSummary
select top 5 * from tbl_ServerProperties
select top 5 * from tbl_Sys_Configurations
select top 5 * from tbl_StartupParameters
select top 5 * from tbl_DatabaseFiles
select top 5 * from tbl_SysDatabases
select top 5 * from tbl_TraceFlags
select top 5 * from tbl_XEvents
select top 5 * from tbl_availability_groups
select top 5 * from tbl_dm_hadr_availability_replica_states
select top 5 * from tbl_availability_replicas
select top 5 * from tbl_dm_os_sys_info
select top 5 * from tbl_dm_os_nodes
select top 5 * from tbl_Thread_Stats_Snapshot
select top 5 * from tbl_dm_os_schedulers_snapshot
select top 5 * from tbl_Thread_Stats
select top 5 * from tbl_System_Requests
select top 5 * from tbl_sysperfinfo
select top 5 * from tbl_dm_os_latch_stats
select top 5 * from tbl_PlanCache_Stats
select top 5 * from tbl_dm_db_file_space_usage
select top 5 * from tbl_dm_exec_cursors
select top 5 * from tbl_profiler_trace_summary
select top 5 * from tbl_dm_os_ring_buffers_conn
select top 5 * from tbl_ring_buffer_temp
select top 5 * from tbl_trace_event_details
select top 5 * from tbl_dm_os_memory_nodes
select top 5 * from tbl_resource_governor_configuration
select top 5 * from tbl_resource_governor_workload_groups
select top 5 * from tbl_resource_governor_resource_pools
select top 5 * from tbl_dbm_partner_time
select top 5 * from tbl_dbm_timeout_connections
select top 5 * from tbl_dbm_perf_control
select top 5 * from tbl_dbm_perf_connection
select top 5 * from tbl_dbm_perf_executions
select top 5 * from tbl_database_mirroring
select top 5 * from tbl_dm_db_mirroring_connections
select top 5 * from tbl_DiagInfo
select top 5 * from tbl_dm_db_stats_properties
select top 5 * from tbl_DisabledIndexes
select top 5 * from tbl_QDS_Query_Stats
select top 5 * from tbl_query_store_runtime_stats_interval
select top 5 * from tbl_query_store_runtime_stats
select top 5 * from tbl_query_store_runtime_stats_interval
select top 5 * from tbl_query_store_query
select top 5 * from tbl_query_store_query_text
select top 5 * from tbl_query_store_plan
select top 5 * from tbl_dm_xtp_gc_stats
select top 5 * from tbl_xtp_gc_queue_stats
select top 5 * from tbl_db_xtp_table_memory_stats
select top 5 * from tbl_xtp_system_memory_consumers
select top 5 * from tbl_dm_os_performance_counters
select top 5 * from tbl_high_cpu_queries
select top 5 * from tbl_server_times
select top 5 * from tbl_database_options
select top 5 * from tbl_db_TDE_Info
select top 5 * from tbl_server_audit_status
select top 5 * from tbl_Top10_CPU_Consuming_Procedures
select top 5 * from tbl_Top10_CPU_Consuming_Triggers
select top 5 * from tbl_Hist_Top10_CPU_Queries_ByQueryHash
select top 5 * from tbl_Hist_Top10_LogicalReads_Queries_ByQueryHash
select top 5 * from tbl_Hist_Top10_ElapsedTime_Queries_ByQueryHash
select top 5 * from tbl_Hist_Top10_CPU_Queries_by_Planhash_and_Queryhash
select top 5 * from tbl_Hist_Top10_LogicalReads_Queries_by_Planhash_and_Queryhash
select top 5 * from tbl_Hist_Top10_ElapsedTime_Queries_by_Planhash_and_Queryhash
select top 5 * from tbl_hadron_replica_info
select top 5 * from tbl_availability_groups
select top 5 * from tbl_hadr_cluster
select top 5 * from tbl_hadr_cluster_members
select top 5 * from tbl_hadr_cluster_networks
select top 5 * from tbl_availability_replicas