classdef ScopeVNS
    % Functions that are useful with ScopeVNS data
    
    methods (Static)
        
        function data = WholeDataset_CombineSameDaySessions ( data )
            
            for r = 1:length(data)
                data(r).Session = ScopeVNS.CombineSameDaySessions(data(r).Session);
            end
            
        end
        
        function new_list = CombineSameDaySessions ( sessions )
            % This function takes an array of sessions and combines all sessions together that are from the same day.
            new_list = [];
            unique_dates = unique([sessions.TimeStamp]);
            
            while (~isempty(unique_dates))
                
                %Pop a date from the queue
                pop_date = unique_dates(1);
                unique_dates(1) = [];
                
                %Create an empty session that will be used to store all sessions of this date
                new_session = [];
                
                %Grab all sessions of this date
                session_indices = find([sessions.TimeStamp] == pop_date);
                sessions_of_this_date = sessions(session_indices);
                
                %Fill in some basic parameters
                new_session.RatName = sessions_of_this_date(1).RatName;
                new_session.Booth = 'NA';
                new_session.ScopeSerialCode = 'NA';
                new_session.MicrosPerSample = sessions_of_this_date(1).MicrosPerSample;
                new_session.TimeStamp = sessions_of_this_date(1).TimeStamp;
                new_session.MedianMaxVoltage = NaN;
                new_session.MedianMinVoltage = NaN;
                new_session.MedianPk2Pk = NaN;
                new_session.NumberOfStimsDetected = NaN;
                new_session.Stims = [];
                
                for i = 1:length(sessions_of_this_date)
                    for s = 1:size(sessions_of_this_date(i).Stims, 1)
                        new_session.Stims = [new_session.Stims; sessions_of_this_date(i).Stims(s, :)];
                    end
                end
                
                new_list = [new_list new_session];
                
            end
            
        end
        
        function PlotSession ( session )
            % Plots an individual ScopeVNS session
            figure;
            hold on;
            for r = 1:size(session.Stims, 1)
                plot(session.Stims(r, :));
            end
        end
        
        function result = IsSignalGoodStimulation ( signal )
            % Given a stimulus pulse as a parameter, this method returns a value indicating whether that stimulus pulse was "good", "bad", or "noise".
            % This function returns one of the following values
            % 0 = not recognized as a stimulation
            % 1 = recognized as a GOOD stimulation
            % 2 = recognized as a BAD stimulation

            result = 0;

            %First, verify that the signal is at least 40 samples
            if (length(signal) >= 40)

                %Now, check to see that we have a voltage increase at around sample 10
                rising_edge_present = (signal(12) >= 1 && signal(12) > signal(8));
                falling_edge_present = (signal(15) >= 1 && signal(25) <= -1);
                return_to_baseline_present = (signal(35) >= signal(30));
                max_voltage = max(signal);
                min_voltage = min(signal);
                pk2pk_voltage = max_voltage - min_voltage;

                if (rising_edge_present && falling_edge_present)
                    if (return_to_baseline_present && max_voltage < 20 && min_voltage > -20 && pk2pk_voltage < 40)
                        result = 1;
                    else
                        result = 2;
                    end
                end
            end
        end
        
        function pk2pk = GetMaximalPeakToPeakVoltage ( session )
            % This function returns the maximal peak-to-peak voltage from all stimuli in a session.
            pk2pk = NaN;
            ppd = [];
            for r = 1:size(session.Stims, 1)
                m1 = max(session.Stims(r, :));
                m2 = min(session.Stims(r, :));
                p = m1 - m2;
                ppd = [ppd p];
            end
            pk2pk = max(ppd);
        end
    end
    
end

