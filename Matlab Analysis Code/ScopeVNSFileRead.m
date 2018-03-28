function data = ScopeVNSFileRead( file )

%Make data an empty variable
data = [];

%Open the file for reading
fid = fopen(file, 'r');

%Get the scope vns file version
line_data = fgetl(fid);

%Split the string into its parts
line_data_split = strsplit(line_data, ':');
if (~isempty(line_data_split) && length(line_data_split) > 1)
    version = str2double(line_data_split(2));
    if (version == 4)
        
        line_data_split = strsplit(fgetl(fid), ':');
        data.RatName = strtrim(line_data_split{2});
        
        line_data_split = strsplit(fgetl(fid), ':');
        data.Booth = strtrim(line_data_split{2});
        
        line_data_split = strsplit(fgetl(fid), ':');
        data.ScopeSerialCode = strtrim(line_data_split{2});
        
        line_data_split = strsplit(fgetl(fid), ':');
        data.ScopeChannel = strtrim(line_data_split{2});
        
        line_data_split = strsplit(fgetl(fid), ':');
        data.MicrosPerSample = str2double(line_data_split(2));
        
        line_data_split = strsplit(fgetl(fid), ':');
        data.TimeStamp = datenum(strtrim(line_data_split{2}));
        
        line_data_split = strsplit(fgetl(fid), ':');
        data.MedianMaxVoltage = str2double(line_data_split(2));
        
        line_data_split = strsplit(fgetl(fid), ':');
        data.MedianMinVoltage = str2double(line_data_split(2));
        
        line_data_split = strsplit(fgetl(fid), ':');
        data.MedianPk2Pk = str2double(line_data_split(2));
        
        line_data_split = strsplit(fgetl(fid), ':');
        num_stims = str2double(line_data_split(2));
        data.NumberOfStimsDetected = num_stims;
        
        data.Stims = [];
        data.StimTimes = [];
        for i = 1:num_stims
            line_data_split = strsplit(fgetl(fid), ',');
            stim_train_str = line_data_split(2:end);
            stim_train = str2double(stim_train_str);
            data.Stims = [data.Stims; stim_train];
            
            %Fetch the timestamp of this stim
            stim_timestamp_full_str = line_data_split{1};
            stim_timestamp_sep1 = strsplit(stim_timestamp_full_str, ':');
            stim_timestamp_date_sep = strsplit(stim_timestamp_sep1{1}, '-');
            stim_timestamp_time_sep = stim_timestamp_sep1(2:end);
            d = str2double(stim_timestamp_date_sep);
            t = str2double(stim_timestamp_time_sep);
            s = t(3) + ((t(4) / 1000) / 1000);
            dv = [d(1) d(2) d(3) t(1) t(2) s];
            dn = datenum(dv);
            data.StimTimes = [data.StimTimes; dn];
        end
        
    end
end

fclose(fid);

end
































