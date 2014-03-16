//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSISender.h"

@interface KSTerminatedMessageSender : NSObject <KSISender>

- (id)initWithTerminator:(NSString *)terminator;

@end
